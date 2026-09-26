using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web;

namespace TechStackLearningHub.Helpers
{
    /// <summary>
    /// Appends exceptions to a text file under App_Data. ASP.NET blocks HTTP
    /// access to App_Data by default, so the log cannot be downloaded by a
    /// visitor. Path is configurable via the ErrorLogPath appSetting in
    /// Web.config.
    ///
    /// Never throws. A logger that throws would replace the user's real error
    /// with a second, unrelated one, which is strictly worse than losing a log
    /// line.
    /// </summary>
    public static class ErrorLogger
    {
        // Two threads appending to the same file at the same instant
        // interleave their bytes and corrupt the entry. lock() lets exactly
        // one writer inside at a time.
        private static readonly object _gate = new object();

        public static void Log(Exception ex, string context)
        {
            if (ex == null) return;

            try
            {
                string path = ResolvePath();
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var sb = new StringBuilder();
                sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss}  [{1}]", DateTime.Now, context);
                sb.AppendLine();
                sb.AppendLine(ex.ToString());
                sb.AppendLine(new string('-', 70));

                lock (_gate)
                {
                    File.AppendAllText(path, sb.ToString());
                }
            }
            catch
            {
                // Swallowed on purpose - see the class comment.
            }
        }

        /// <summary>
        /// Public so a page or admin screen can offer a "download log" link to
        /// a genuinely authenticated admin. There is no such link today; this
        /// exists so adding one does not require reimplementing the path logic.
        /// </summary>
        public static string ResolvePath()
        {
            string configured = ConfigurationManager.AppSettings["ErrorLogPath"];
            if (string.IsNullOrWhiteSpace(configured))
                configured = "~/App_Data/errors.log";

            HttpContext context = HttpContext.Current;
            if (context == null)
                return configured;

            return context.Server.MapPath(configured);
        }
    }
}
