using System;
using System.Web;
using System.Web.Security;
using TechStackLearningHub.Data_Access_Layer;
using TechStackLearningHub.Helpers;
using TechStackLearningHub.Models;

namespace TechStackLearningHub.BLL
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

        public bool Register(string username, string email, string password)
        {
            // Fail fast: reject duplicates before spending any effort on hashing.
            if (_userDAL.SelectByUsername(username) != null)
                throw new ValidationException("That username is already taken.");

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

            _userDAL.Insert(user);
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
