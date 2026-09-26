using System;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;

namespace TechStackLearningHub.Web.Helpers
{
    /// <summary>
    /// Shared plumbing for the admin CRUD pages: building the confirmation
    /// script for a destructive action, and keeping the success and error
    /// message regions mutually exclusive.
    ///
    /// The confirmation text embeds a value taken from the database, so it is
    /// escaped before it reaches the script. Without that, a course called
    /// Bob's Basics would terminate the string and turn the rest of the title
    /// into script.
    /// </summary>
    public static class AdminUi
    {
        // Long enough to identify the row, short enough to stay readable in a
        // confirmation dialog.
        private const int MaxNameLength = 60;

        /// <summary>
        /// Builds "return confirm('...');" for an OnClientClick attribute.
        /// {0} in <paramref name="template"/> is replaced with the escaped,
        /// truncated item name.
        /// </summary>
        public static string Confirm(string template, object name)
        {
            string message = (template ?? string.Empty).Replace("{0}", "'" + Shorten(name) + "'");
            return "return confirm('" + EscapeForSingleQuotedJs(message) + "');";
        }

        private static string Shorten(object name)
        {
            string text = Convert.ToString(name, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(text))
                return "this item";

            text = text.Trim();
            return text.Length <= MaxNameLength ? text : text.Substring(0, MaxNameLength) + "...";
        }

        /// <summary>
        /// Escapes a value for use inside a single-quoted JavaScript string
        /// literal. Also encodes the HTML-significant characters, so the value
        /// cannot break out of the attribute it is written into.
        /// </summary>
        private static string EscapeForSingleQuotedJs(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var sb = new StringBuilder(value.Length + 16);
            foreach (char c in value)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '\'': sb.Append("\\'"); break;
                    case '"': sb.Append("\\u0022"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '<': sb.Append("\\u003c"); break;
                    case '>': sb.Append("\\u003e"); break;
                    case '&': sb.Append("\\u0026"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Shows a success message and clears any error, so the two regions can
        /// never report contradictory outcomes at the same time.
        /// </summary>
        public static void Success(Label error, Label success, string message)
        {
            if (error != null)
                error.Visible = false;
            if (success == null)
                return;

            success.Text = HttpUtility.HtmlEncode(message);
            success.Visible = true;
        }

        /// <summary>
        /// Reports a failure. A ValidationException is authored for the admin
        /// and safe to render; anything else is a fault, so it is logged and
        /// replaced with a fixed message.
        /// </summary>
        public static void Error(Label error, Label success, Exception ex, string fallback, string context)
        {
            if (success != null)
                success.Visible = false;
            if (error == null)
                return;

            var validation = ex as ValidationException;
            if (validation != null)
            {
                error.Text = HttpUtility.HtmlEncode(validation.Message);
            }
            else
            {
                ErrorLogger.Log(ex, context);
                error.Text = fallback;
            }
            error.Visible = true;
        }
    }
}
