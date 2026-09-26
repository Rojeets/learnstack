using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using TechStackLearningHub.Web.BLL;

namespace TechStackLearningHub.Web.Helpers
{
    /// <summary>
    /// Delivers password-reset links over SMTP using the built-in
    /// System.Net.Mail.SmtpClient. That client has shipped with the framework
    /// since 1.1, so this needs no NuGet package and adds none.
    ///
    /// CONFIGURATION. Every setting is resolved the same way, in this order:
    ///
    ///   1. an environment variable named after the key (SMTP_HOST, ...)
    ///   2. the appSettings entry of the same name in Web.config
    ///   3. otherwise: not configured
    ///
    /// The environment variable wins deliberately. Web.config is tracked by
    /// git, so a credential typed into it becomes a committed secret the moment
    /// anybody pushes. Web.config therefore carries empty placeholders - which
    /// also keeps a fresh clone working: the site starts and reports "SMTP is
    /// not configured" instead of failing to load its own configuration because
    /// an external config file is missing. Smtp.config.template documents the
    /// keys for humans; the app never reads it, and it must never be filled in
    /// and committed.
    ///
    /// FAILURE IS LOUD, ON PURPOSE. Missing configuration throws
    /// InvalidOperationException and a refused or unreachable relay throws
    /// SmtpException; nothing is swallowed here. The caller
    /// (AuthBLL.RequestPasswordReset) catches whatever comes out, logs it, and
    /// still shows the visitor one neutral "if that address exists we sent a
    /// link" message - so throwing costs the site nothing and tells an attacker
    /// nothing about whether the address was real. It also means a reset that
    /// was never delivered is a line in the error log, instead of a user
    /// waiting for mail that will never arrive.
    ///
    /// THE RESET URL IS NEVER LOGGED. A reset URL is a live credential for as
    /// long as its token is redeemable: whoever holds the mail, a compromised
    /// mailbox or a forwarded message can take the account over. So the URL
    /// appears in exactly one place in this class - the message body - and is
    /// never interpolated into an exception message or handed to ErrorLogger.
    /// This class does not call ErrorLogger at all: a log line written here is
    /// one more place a URL could escape from, and the caller already logs the
    /// failure with the context that matters.
    /// </summary>
    public class SmtpResetNotifier : IResetNotifier
    {
        // The whole configuration vocabulary. Key name and environment
        // variable name are deliberately the same string, so "SMTP_HOST" is
        // one setting with two places it can be supplied from.
        private const string HostKey = "SMTP_HOST";
        private const string PortKey = "SMTP_PORT";
        private const string UserKey = "SMTP_USER";
        private const string PasswordKey = "SMTP_PASSWORD";
        private const string EnableSslKey = "SMTP_ENABLE_SSL";
        private const string FromKey = "SMTP_FROM";
        private const string BaseUrlKey = "PASSWORD_RESET_BASE_URL";

        private const string Subject = "Reset your TechStack Learning Hub password";

        // SmtpClient.Timeout defaults to 100 seconds. Sending is synchronous
        // inside a web request, so an unreachable relay would hold a thread and
        // the visitor for a minute and a half before failing. 15 seconds is
        // ample for a healthy SMTP conversation and short enough that a dead
        // host becomes a log line instead of a pile of tied-up requests.
        private const int TimeoutMilliseconds = 15000;

        /// <summary>
        /// Explicit because AuthBLL constructs this directly; it documents at
        /// the class that configuration comes from the environment, not from
        /// constructor arguments.
        /// </summary>
        public SmtpResetNotifier()
        {
        }

        /// <summary>
        /// Sends one plain-text message containing the reset link. Delivers or
        /// throws - see the class comment for why, and for why the URL never
        /// appears in anything this method constructs.
        /// </summary>
        public void SendResetLink(string toEmail, string displayName, string resetUrl)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("A recipient email address is required.", "toEmail");

            // A blank URL here means the caller built a broken link, which is a
            // programming error rather than a configuration one - and an empty
            // link sitting in a user's inbox is indistinguishable from a reset
            // that did not work.
            if (string.IsNullOrWhiteSpace(resetUrl))
                throw new ArgumentException("A reset URL is required.", "resetUrl");

            SmtpSettings settings = LoadSettings();

            using (var message = new MailMessage())
            using (var client = new SmtpClient())
            {
                message.From = new MailAddress(settings.From);
                message.Subject = Subject;

                // Plain text, deliberately. An HTML body invites a lookalike
                // phishing page dressed up to match, and a bare URL the reader
                // can read and check before clicking is both safer and simpler.
                message.IsBodyHtml = false;
                message.Body = BuildBody(displayName, resetUrl);

                // The MailAddress overload, never a hand-assembled
                // "Name <addr>" string. The string form is a header-injection
                // vector - a newline inside displayName appends arbitrary
                // headers to the message - and it throws on a long list of
                // perfectly ordinary names. This parses and validates instead.
                message.To.Add(string.IsNullOrWhiteSpace(displayName)
                    ? new MailAddress(toEmail.Trim())
                    : new MailAddress(toEmail.Trim(), displayName.Trim()));

                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Host = settings.Host;
                client.Port = settings.PortNumber;

                // STARTTLS, which is what port 587 does. System.Net.Mail's
                // SmtpClient cannot do implicit TLS on 465, so that port needs
                // a different client rather than a different setting here.
                client.EnableSsl = settings.SslEnabled;
                client.Timeout = TimeoutMilliseconds;

                // Never fall back to the identity of the worker process: that
                // would quietly offer the app pool's own account to the relay
                // and hide a missing SMTP_USER behind a baffling auth error.
                client.UseDefaultCredentials = false;
                if (settings.UseAuthentication)
                    client.Credentials = new NetworkCredential(settings.User, settings.Password);

                // No try/catch, and no wrapping. The SmtpException already
                // names the server and the reason, which is the diagnostic the
                // caller logs. Rewriting it "to add context" is precisely where
                // a reset URL would get interpolated into an exception message
                // that ends up in a log file, so the original is allowed to
                // propagate untouched.
                client.Send(message);
            }
        }

        /// <summary>
        /// The body of the message.
        ///
        /// The link is on a line of its own and nothing wraps it, so it stays
        /// readable and stays exactly what the user is asked to verify. The
        /// expiry is quoted from AuthBLL rather than written as a literal, so
        /// the lifetime the email promises and the lifetime the token actually
        /// enforces cannot drift apart. That reference runs Helpers -> BLL,
        /// which SqlErrorHelper already does for the same reason: a policy
        /// stated in one place cannot be contradicted by the mail about it.
        /// </summary>
        private static string BuildBody(string displayName, string resetUrl)
        {
            // A real account always has a username, but the fallback is free
            // and reads better than a greeting with a hole in it.
            string name = string.IsNullOrWhiteSpace(displayName)
                ? "there"
                : displayName.Trim();

            var body = new StringBuilder();
            body.Append("Hi ").Append(name).Append(",").AppendLine();
            body.AppendLine();
            body.AppendLine("Someone asked to reset the password on your TechStack Learning Hub account.");
            body.AppendLine("Open the link below to choose a new one:");
            body.AppendLine();
            body.AppendLine(resetUrl);
            body.AppendLine();
            body.Append("Please use it straight away. The link expires ")
                .Append(AuthBLL.ResetTokenLifetimeMinutes)
                .AppendLine(" minutes after this email was sent, it can be used only once, and it stops");
            body.AppendLine("working as soon as another reset link is requested.");
            body.AppendLine();
            body.AppendLine("If you did not ask for this, please ignore this email. Your password has not");
            body.AppendLine("changed and there is nothing you need to do.");
            body.AppendLine();
            body.AppendLine("-- TechStack Learning Hub");

            return body.ToString();
        }

        /// <summary>
        /// Resolves every key and refuses to guess. All missing keys are named
        /// in one message: fixing configuration one exception at a time, with a
        /// reset attempt in between, is a slow way to learn a list.
        /// </summary>
        private static SmtpSettings LoadSettings()
        {
            var settings = new SmtpSettings
            {
                Host = Resolve(HostKey),
                PortText = Resolve(PortKey),
                EnableSslText = Resolve(EnableSslKey),
                From = Resolve(FromKey),
                BaseUrl = Resolve(BaseUrlKey),
                User = Resolve(UserKey),
                Password = Resolve(PasswordKey)
            };

            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(settings.Host)) missing.Add(HostKey);
            if (string.IsNullOrWhiteSpace(settings.PortText)) missing.Add(PortKey);
            if (string.IsNullOrWhiteSpace(settings.EnableSslText)) missing.Add(EnableSslKey);
            if (string.IsNullOrWhiteSpace(settings.From)) missing.Add(FromKey);

            // PASSWORD_RESET_BASE_URL is AuthBLL's job to consume, not this
            // class's - the URL arrives here already built. It is still
            // required, because a missing base URL means every link this
            // notifier is about to send points at somewhere the recipient
            // cannot use. Failing here names the key in the log; sending mail
            // that cannot work only produces support questions.
            if (string.IsNullOrWhiteSpace(settings.BaseUrl)) missing.Add(BaseUrlKey);

            if (missing.Count > 0)
                throw new InvalidOperationException(
                    "Password reset email is not configured. Missing: "
                    + string.Join(", ", missing.ToArray())
                    + ". Set each one as an environment variable of the same name - environment "
                    + "variables are read first so a password is never committed to Web.config - or "
                    + "as an appSettings entry. See Smtp.config.template for what each key means.");

            settings.PortNumber = ParsePort(settings.PortText);
            settings.SslEnabled = ParseSsl(EnableSslKey, settings.EnableSslText);

            // Half a credential pair is a mistake, not a weaker configuration,
            // and it fails confusingly: relaying with an empty username looks
            // like a relay that accepts mail from anyone, and relaying with an
            // empty password looks like a wrong password.
            bool hasUser = !string.IsNullOrWhiteSpace(settings.User);
            bool hasPassword = !string.IsNullOrWhiteSpace(settings.Password);
            if (hasUser != hasPassword)
                throw new InvalidOperationException(
                    "Password reset email credentials are half configured: "
                    + (hasUser ? PasswordKey : UserKey)
                    + " is empty. Set both to authenticate against the relay, or clear both for a "
                    + "relay that needs no authentication.");

            settings.UseAuthentication = hasUser;
            return settings;
        }

        /// <summary>
        /// One key, two sources, environment variable first.
        ///
        /// The value is trimmed because it arrives from hand-edited files and
        /// machine-level environment variables, where a trailing space or
        /// newline is routine - and " smtp.example.com" is not a host, while
        /// the resulting connection error blames the network.
        /// </summary>
        private static string Resolve(string key)
        {
            string value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(value))
                value = ConfigurationManager.AppSettings[key];

            return value == null ? null : value.Trim();
        }

        private static int ParsePort(string raw)
        {
            int port;
            try
            {
                // InvariantCulture explicitly: int.Parse(string) alone follows
                // the current culture, and a machine whose regional settings
                // use different digits or separators must not reinterpret
                // "587".
                port = int.Parse(raw, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException(
                    PortKey + " must be a whole number, but was \"" + raw + "\".");
            }
            catch (OverflowException)
            {
                throw new InvalidOperationException(
                    PortKey + " is too large to be a port, but was \"" + raw + "\".");
            }

            if (port < 1 || port > 65535)
                throw new InvalidOperationException(
                    PortKey + " must be between 1 and 65535, but was "
                    + port.ToString(CultureInfo.InvariantCulture) + ".");

            return port;
        }

        private static bool ParseSsl(string key, string raw)
        {
            bool enabled;

            // bool.Parse has no culture-sensitive overload - "true" and "false"
            // are the only accepted spellings in every culture - so TryParse is
            // the non-throwing form of the same parse.
            if (!bool.TryParse(raw, out enabled))
                throw new InvalidOperationException(
                    key + " must be true or false, but was \"" + raw + "\".");

            return enabled;
        }

        /// <summary>
        /// One send's worth of configuration, resolved and validated before
        /// anything is sent. A holder rather than a pile of out parameters, so
        /// the compiler enforces that nothing is used half-resolved.
        /// </summary>
        private sealed class SmtpSettings
        {
            public string Host { get; set; }
            public string PortText { get; set; }
            public string EnableSslText { get; set; }
            public int PortNumber { get; set; }
            public bool SslEnabled { get; set; }
            public string From { get; set; }
            public string User { get; set; }
            public string Password { get; set; }
            public bool UseAuthentication { get; set; }

            /// <summary>
            /// Validated but not otherwise used here - AuthBLL owns the base
            /// URL. Kept so the required-key check covers it and so the key
            /// names live in one place.
            /// </summary>
            public string BaseUrl { get; set; }
        }
    }
}
