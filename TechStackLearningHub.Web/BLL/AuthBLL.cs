using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using TechStackLearningHub.Web.Data_Access_Layer;
using TechStackLearningHub.Web.Helpers;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.BLL
{
    // Login outcome. Deactivated is a distinct state so the calling page can
    // show an accurate message instead of "invalid credentials".
    public class LoginResult
    {
        public bool Success { get; set; }
        public bool Deactivated { get; set; }
        public User User { get; set; }

        public static LoginResult Failure()
        {
            return new LoginResult { Success = false, Deactivated = false };
        }

        public static LoginResult AccountDeactivated()
        {
            return new LoginResult { Success = false, Deactivated = true };
        }

        public static LoginResult Succeeded(User user)
        {
            return new LoginResult { Success = true, User = user };
        }
    }

    public class AuthBLL
    {
        private readonly UserDAL _userDAL = new UserDAL();

        // Password cryptography lives in Helpers/PasswordHelper, not here.
        // The derivation parameters are FROZEN and must stay identical to the
        // values documented in App_Data/TechStackLearningHub.sql so that the
        // seeded admin/student accounts validate against this exact scheme.
        public const int PBKDF2Iterations = PasswordHelper.PBKDF2Iterations;
        public const int SaltSizeBytes = PasswordHelper.SaltSizeBytes;
        public const int HashSizeBytes = PasswordHelper.HashSizeBytes;

        // ------------------------------------------------------------------
        // Password-reset configuration.
        //
        // Overridable consts, not literals buried in the methods, so tightening
        // a policy is a one-line change that every caller of that policy sees.
        // ------------------------------------------------------------------

        /// <summary>CSPRNG bytes per reset token. 32 bytes = 256 bits.</summary>
        private const int ResetTokenSizeBytes = 32;

        /// <summary>How long a reset link stays redeemable. 30 minutes.</summary>
        public const int ResetTokenLifetimeMinutes = 30;

        /// <summary>
        /// Where the reset page lives, relative to the base URL. The one place
        /// to change if the page is ever moved or renamed; the base URL and the
        /// page path are assembled together in BuildResetUrl.
        /// </summary>
        private const string ResetPagePath = "Account/ResetPassword.aspx";

        /// <summary>
        /// Used only when the PasswordResetBaseUrl appSetting is missing or
        /// blank, so the code runs (and fails visibly, in the log) before the
        /// key is configured rather than throwing. Correct on a machine serving
        /// the site from the root of localhost and wrong everywhere else,
        /// which is why the setting must be supplied per environment - a reset
        /// link is only useful if it points at the site that issued it.
        /// </summary>
        private const string DefaultResetBaseUrl = "http://localhost/";

        /// <summary>
        /// The single user-facing message for every way a link can fail: never
        /// issued, already spent, or expired. Three different answers would
        /// turn the reset form into a probe - "already spent" versus "never
        /// existed" is exactly the distinction an attacker guessing tokens
        /// wants, and it tells them nothing they could not learn by waiting.
        /// </summary>
        private const string InvalidResetLinkMessage =
            "This reset link is invalid or has expired. Please request a new one.";

        // Fixed throwaway credentials for PerformDummyPasswordVerification:
        // 16 random bytes of salt and 32 of hash, generated once, never
        // associated with an account and never compared against anything a
        // user can supply. The sizes are what matter - they make the dummy
        // derivation cost exactly what a real one costs.
        private const string DummySaltBase64 = "3L698EOyQumAyTA/kpaacg==";
        private const string DummyHashBase64 = "BH+kOuTEhPHe4cXG+w/mLAd5XP+QRq2A1VaF/OwL7Oo=";
        private const string DummyProbePassword = "not-a-real-password";

        public bool Register(string username, string email, string password)
        {
            // Fail fast: reject duplicates before spending any effort on hashing.
            if (_userDAL.SelectByUsername(username) != null)
                throw new ValidationException("That username is already taken.");

            // Shared with CompletePasswordReset so the rule that decides whether
            // a password is acceptable cannot drift between the two ways an
            // account's password gets set.
            ValidatePassword(password);

            byte[] salt = PasswordHelper.GenerateSalt();
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = PasswordHelper.DeriveHashBase64(password, salt),
                PasswordSalt = Convert.ToBase64String(salt),
                // Registration can never self-assign a role: always Student.
                RoleID = _userDAL.GetRoleIdByName("Student"),
                IsActive = true
            };
            if (user.RoleID <= 0)
                throw new InvalidOperationException("The Student role is not configured.");

            try
            {
                _userDAL.Insert(user);
            }
            catch (SqlException sqlEx)
            {
                // The check above rejects a duplicate username, but two
                // simultaneous registrations can still race past it, and Email
                // has its own UNIQUE index that nothing pre-checks. Translate
                // both into one authored message instead of letting a raw
                // SqlException surface as a generic failure.
                ValidationException vex = SqlErrorHelper.Translate(
                    sqlEx, uniqueMessage: "That username or email address is already registered.");
                if (vex != null) throw vex;

                ErrorLogger.Log(sqlEx, "AuthBLL.Register");
                throw new ValidationException("A database error occurred. Please try again.");
            }
            return true;
        }

        public LoginResult Login(string username, string password)
        {
            User user = _userDAL.SelectByUsername(username);
            if (user == null)
                return LoginResult.Failure();

            if (!user.IsActive)
                return LoginResult.AccountDeactivated();

            byte[] salt = Convert.FromBase64String(user.PasswordSalt);
            byte[] expected = Convert.FromBase64String(user.PasswordHash);
            byte[] actual = PasswordHelper.DeriveHash(password, salt);

            // Constant-time comparison, so the time taken to reject a wrong
            // password cannot be used to guess the hash one byte at a time.
            if (!PasswordHelper.FixedTimeEquals(actual, expected))
                return LoginResult.Failure();

            user.RoleName = _userDAL.GetRoleNameById(user.RoleID);
            user.PasswordHash = null;
            user.PasswordSalt = null;
            return LoginResult.Succeeded(user);
        }

        public void CreateSessionForUser(User user)
        {
            HttpContext context = HttpContext.Current;
            if (context == null || context.Session == null)
                throw new InvalidOperationException("No HTTP session is available.");

            context.Session["UserID"] = user.UserID;
            context.Session["Username"] = user.Username;
            context.Session["RoleName"] = user.RoleName ?? "Student";

            // Cookie enables web.config <authorization> rules; Session backs
            // fast per-page lookups. Both are set so both gatekeepers work.
            FormsAuthentication.SetAuthCookie(user.Username, false);
        }

        public void Logout()
        {
            HttpContext context = HttpContext.Current;
            if (context == null || context.Session == null)
                return;

            context.Session.Clear();
            context.Session.Abandon();
            FormsAuthentication.SignOut();
        }

        // ------------------------------------------------------------------
        // Password reset.
        //
        // Two public entry points, and both of them are security-sensitive in
        // ways that are easy to undo by accident:
        //
        //   RequestPasswordReset  must be enumeration resistant. It is the
        //     only unauthenticated endpoint in the app that asks a question
        //     about an account, so a "no such address" answer is a free
        //     account-enumeration oracle. Both the unknown-address and the
        //     deactivated-account cases therefore do no token work, send no
        //     mail, and return normally; the page always renders the same
        //     neutral confirmation.
        //
        //   CompletePasswordReset  must be single-use. The "AND UsedAt IS NULL"
        //     guard in the DAL plus the transaction below are what make that
        //     true under concurrency.
        //
        // The emailed token is CSPRNG output and only its SHA-256 is stored,
        // so the database is never a list of working reset links.
        // ------------------------------------------------------------------

        /// <summary>
        /// Starts the reset flow for an address. Returns void and never throws
        /// for an unknown, deactivated or unreachable-mail case, because the
        /// caller has nothing useful to tell the visitor: the page shows one
        /// neutral confirmation either way.
        /// </summary>
        public void RequestPasswordReset(string email)
        {
            // Trimming is not cosmetic. The address as typed often carries a
            // trailing space or newline, and a silent mismatch here would look
            // exactly like "no such account" to the person who owns it.
            string address = (email ?? string.Empty).Trim();
            if (address.Length == 0)
            {
                // The page's own validators own the user-facing message for a
                // blank field; there is no account question to answer here.
                return;
            }

            User user = _userDAL.SelectByEmail(address);

            // ENUMERATION RESISTANT. An unknown address and a known-but-
            // deactivated one are folded together on purpose: neither writes a
            // token and neither sends anything, so no observable difference can
            // be made between them. Both are also the SAME case as far as the
            // visitor is concerned - the page says "if that address has an
            // account, a link is on its way" in every branch.
            if (user == null || !user.IsActive)
            {
                PerformDummyPasswordVerification();
                return;
            }

            // One live token per user. Requesting a new link revokes the old
            // ones, so a link forwarded to an attacker before the user changed
            // their mind stops working the moment the user asks again.
            DateTime now = DateTime.Now;
            _userDAL.InvalidateOutstandingResetTokens(user.UserID, now);

            // The token exists in plaintext exactly once, right here, and only
            // the digest below is persisted. The emailed link is the only copy.
            string plainToken = GenerateResetToken();
            _userDAL.InsertResetToken(new PasswordResetToken
            {
                UserID = user.UserID,
                TokenHash = HashResetToken(plainToken),
                CreatedDate = now,
                ExpiresAt = now.AddMinutes(ResetTokenLifetimeMinutes)
            });

            try
            {
                CreateResetNotifier().SendResetLink(user.Email, user.Username, BuildResetUrl(plainToken));
            }
            catch (Exception ex)
            {
                // Deliberately swallowed, and this is the important part: if
                // SMTP is down, throwing - or even logging-and-rethrowing a
                // different exception - would make this call behave visibly
                // differently for a real address than for a fake one, which
                // hands an attacker the enumeration oracle this whole method
                // exists to withhold. The token stays in the table and simply
                // expires unused; the visitor gets the same neutral page.
                //
                // The exception itself is logged, not the URL, which is a live
                // credential for the token's lifetime.
                ErrorLogger.Log(ex, "AuthBLL.RequestPasswordReset");
            }
        }

        /// <summary>
        /// Redeems a reset link and sets a new password. Throws
        /// ValidationException with an authored message for every rejection.
        /// </summary>
        public void CompletePasswordReset(string token, string newPassword)
        {
            // Trimmed for the same reason the address is: a mail client or a
            // hand-typed paste can easily add whitespace, and a token that is
            // merely surrounded by spaces must still work. Trimming is safe
            // because the alphabet below contains no whitespace.
            string suppliedToken = (token ?? string.Empty).Trim();

            // One lookup, one message. Absent, already spent and expired all
            // produce InvalidResetLinkMessage, so nothing about the token can
            // be learned from which case applied.
            ResetTokenLookup lookup = _userDAL.SelectResetTokenByHash(HashResetToken(suppliedToken));
            if (lookup == null || lookup.Token == null || !lookup.Token.IsRedeemable)
                throw new ValidationException(InvalidResetLinkMessage);

            // A distinct message here is safe, and a silent success would not
            // be: the caller is already holding a valid, unspent, unexpired
            // token, so it has proved it controls that address and learns
            // nothing new from "deactivated". The user does, so they are told
            // the truth and pointed at the administrator who can re-enable it.
            if (!lookup.IsActive)
                throw new ValidationException(
                    "This account has been deactivated. Please contact an administrator.");

            // Checked BEFORE the token is stamped, so a rejected password
            // leaves the link usable and a typo does not cost the user a
            // second request.
            ValidatePassword(newPassword);

            byte[] salt = PasswordHelper.GenerateSalt();
            string newHash = PasswordHelper.DeriveHashBase64(newPassword, salt);
            string newSalt = Convert.ToBase64String(salt);

            int tokenId = lookup.Token.TokenID;
            int userId = lookup.Token.UserID;

            // Atomic, because the alternative states are both security-relevant:
            // a spent token with the old password still in place (a reset that
            // did not happen, on a link the user believes is dead), or a new
            // password with the token still live (a link that can be replayed to
            // reset the account again, and again, until it expires).
            DbHelper.ExecuteInTransaction((conn, tx) =>
            {
                // Compare-and-set. Losing this race means another request
                // redeemed the same link between our read and this write, so
                // 0 rows is a lost race, not a no-op - and rolling back is what
                // keeps the loser from also overwriting the winner's password.
                if (_userDAL.MarkResetTokenUsed(conn, tx, tokenId, DateTime.Now) == 0)
                    throw new ValidationException(InvalidResetLinkMessage);

                // 0 rows means the account was deleted underneath the token -
                // a system fault, not something the visitor can act on, and it
                // must not leave the token stamped with no password changed.
                if (_userDAL.UpdatePassword(conn, tx, userId, newHash, newSalt) == 0)
                    throw new InvalidOperationException(
                        "Password reset could not be applied: the account no longer exists.");

                // The token just spent is already stamped, so the DAL's
                // "UsedAt IS NULL" guard skips it and no special case is
                // needed for "this one" here.
                _userDAL.InvalidateOutstandingResetTokens(conn, tx, userId, DateTime.Now);
            });

            // KNOWN LIMITATION: the account's existing Forms Auth cookie is not
            // revoked. Sessions here are in-process with no store to enumerate,
            // so there is no list of that user's live session ids to walk -
            // the honest options were to leave the cookie valid or to sign
            // everyone out globally, and signing everyone out is a denial of
            // service on a shared demo database. The practical exposure is
            // bounded: the reset was proved by a link only the user holds, and
            // a stolen session cookie dies with the browser session (120
            // minutes, sliding). Doing this properly needs a session store the
            // app does not have.
        }

        /// <summary>
        /// The single password rule, shared by Register and
        /// CompletePasswordReset.
        ///
        /// This application has NO length or complexity policy: registration
        /// enforces only the not-blank rule here, the confirmation match is the
        /// page's job, and inventing a policy inside a password-reset feature
        /// would lock out exactly the users who most need to reset - an old
        /// weak password that no longer satisfies a rule they have never seen
        /// would leave them unable to get back in. What this method does
        /// guarantee is that the rule is stated in ONE place, so adding a
        /// policy later is a single edit that both paths inherit and the two
        /// cannot drift apart.
        /// </summary>
        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ValidationException("Please choose a password.");
        }

        /// <summary>
        /// 32 bytes from the OS CSPRNG, base64url-encoded.
        ///
        /// System.Security.Cryptography.RandomNumberGenerator, not
        /// System.Random: the token is a password reset credential, and
        /// System.Random is a predictable PRNG whose state can be recovered
        /// from a handful of its own outputs. A guessable token is a
        /// guessable password change.
        ///
        /// base64url, not base64, because the token rides in a query string:
        /// '=' padding, '+' and '/' all have to be escaped or altered in
        /// transit, and every mangling step is another way for the link to
        /// arrive broken. Strip the padding, then swap the two characters that
        /// are not URL-safe. The result is the URL-safe alphabet unescaped.
        /// </summary>
        private static string GenerateResetToken()
        {
            var bytes = new byte[ResetTokenSizeBytes];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        /// <summary>
        /// Base64 SHA-256 of the token - the only form that is ever stored or
        /// looked up. Base64 to match how PasswordHelper stores its output and
        /// to fit the NVARCHAR(256) column.
        ///
        /// Deliberately NOT PBKDF2, unlike a password: the input is 256 bits
        /// of CSPRNG output with no entropy an attacker could enumerate, so
        /// there is nothing for a slow KDF to defend against, and a fast digest
        /// keeps the lookup on the indexed equality path.
        /// </summary>
        private static string HashResetToken(string token)
        {
            using (var sha = SHA256.Create())
            {
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(token)));
            }
        }

        /// <summary>
        /// Assembles the emailed link.
        ///
        /// The base URL comes from configuration and NEVER from the current
        /// request. A Host header the attacker controls would otherwise end up
        /// inside an email the victim has every reason to trust, turning
        /// "request a password reset" into "send the victim a link to a host of
        /// my choosing, with a token in it". The request knows nothing about
        /// where it should be trusted to run.
        ///
        /// Read defensively - a blank or absent appSetting falls back to a
        /// documented localhost default instead of throwing, so a missing
        /// configuration entry shows up as a wrong link in the log rather than
        /// as a stack trace on a public page.
        /// </summary>
        private static string BuildResetUrl(string plainToken)
        {
            string baseUrl = ConfigurationManager.AppSettings["PasswordResetBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = DefaultResetBaseUrl;

            // A no-op for base64url output, kept so that a future change to the
            // token alphabet cannot silently put an unescaped '&' or '#' into
            // the query string and truncate the link.
            return baseUrl.TrimEnd('/') + "/" + ResetPagePath
                + "?token=" + HttpUtility.UrlEncode(plainToken);
        }

        /// <summary>
        /// The ONE place AuthBLL names a transport. Everything above deals in
        /// IResetNotifier, so changing delivery - a queue, a different mailer, a
        /// fake in a test - is this one line and nothing else in AuthBLL knows
        /// that SMTP exists.
        /// </summary>
        private static IResetNotifier CreateResetNotifier()
        {
            return new SmtpResetNotifier();
        }

        /// <summary>
        /// One throwaway PBKDF2 derivation, run on the paths where no account
        /// was found.
        ///
        /// REASON: on the happy path this method spends ~100k PBKDF2 iterations
        /// elsewhere in the request cycle, so an address that does not exist
        /// would otherwise return measurably faster than one that does, and
        /// "this one is faster, so this one is not registered" is an
        /// enumeration oracle built out of nothing but a stopwatch. The
        /// derivation is against a fixed throwaway salt and hash that no
        /// account can ever match, so it does the same amount of work and
        /// reaches no branch that depends on the input.
        ///
        /// Honest limit: this closes the CHEAPEST observable difference, not all
        /// of them. The found path also inserts a row and talks to an SMTP
        /// server, and that cost cannot be flattened without giving up the
        /// feature. What actually holds the line is that every path returns
        /// normally with no output difference at all; this only keeps timing
        /// from re-opening what the response already refuses to say.
        /// </summary>
        private static void PerformDummyPasswordVerification()
        {
            byte[] salt = Convert.FromBase64String(DummySaltBase64);
            byte[] expected = Convert.FromBase64String(DummyHashBase64);
            byte[] actual = PasswordHelper.DeriveHash(DummyProbePassword, salt);

            // Result deliberately discarded - the point is the work, not the
            // answer. The throwaway hash cannot match, and FixedTimeEquals is
            // NoInlining/NoOptimization precisely so this call is not optimised
            // away as dead.
            PasswordHelper.FixedTimeEquals(actual, expected);
        }

        // ------------------------------------------------------------------
        // Session read helpers.
        //
        // AuthBLL is the ONLY class that reads or writes Session. Pages call
        // these instead of touching Session directly, so "how do we know who
        // is signed in" lives in exactly one place.
        // ------------------------------------------------------------------

        public static bool IsLoggedIn
        {
            get
            {
                HttpContext context = HttpContext.Current;
                return context != null && context.Session != null && context.Session["UserID"] != null;
            }
        }

        public static int CurrentUserId
        {
            get
            {
                if (!IsLoggedIn) return 0;
                return Convert.ToInt32(HttpContext.Current.Session["UserID"]);
            }
        }

        public static string CurrentUserName
        {
            get { return IsLoggedIn ? HttpContext.Current.Session["Username"] as string : null; }
        }

        public static string CurrentRoleName
        {
            get { return IsLoggedIn ? HttpContext.Current.Session["RoleName"] as string : null; }
        }

        public static bool IsAdmin
        {
            get { return CurrentRoleName == "Admin"; }
        }

        // Guards that REDIRECT rather than throw, matching the behaviour the
        // old App_Start/RoleGuard.cs had. A throwing guard would replace a
        // clean redirect with an unhandled-exception page.
        public static void RequireLogin()
        {
            if (IsLoggedIn) return;
            RedirectToLogin();
        }

        public static void RequireAdmin()
        {
            if (!IsLoggedIn)
            {
                RedirectToLogin();
                return;
            }
            if (IsAdmin) return;
            HttpContext.Current.Response.Redirect("~/Pages/Default.aspx");
        }

        public static void RedirectToLogin()
        {
            HttpContext context = HttpContext.Current;
            if (context == null) return;

            string returnUrl = context.Request.RawUrl;
            context.Response.Redirect(
                "~/Account/Login.aspx?ReturnUrl=" + HttpUtility.UrlEncode(returnUrl));
        }
    }
}
