using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;

namespace TechStackLearningHub.Web.Helpers
{
    /// <summary>
    /// Keeps the address bar in step with a filter the admin just changed.
    ///
    /// These dropdowns re-post-back, so the page behind them re-renders with the
    /// new selection while the URL still describes the old one. Nothing looked
    /// wrong on screen, but refreshing, bookmarking or sharing the link reopened
    /// the previous workspace, and the browser Back button could not undo the
    /// change because no navigation had been recorded.
    ///
    /// The rewrite is done with replaceState rather than a redirect for three
    /// reasons. The dropdown has already posted back and the server has already
    /// re-rendered, so there is nothing left to fetch and a redirect would only
    /// add a second round trip. replaceState leaves no history entry, so Back
    /// still leaves the page instead of walking back through every course the
    /// admin clicked through. And a redirect would have to be a ThreadAbortException
    /// to be safe, which is exactly the fault this codebase stopped logging.
    ///
    /// The URL is built from the selection the server resolved, never from
    /// anything the browser sends, so a crafted postback cannot push an
    /// arbitrary value into the address bar.
    /// </summary>
    public static class UrlSync
    {
        /// <summary>
        /// Points the address bar at this page with <paramref name="nameThenValue"/>
        /// as its query string, dropping any pair whose value is null or empty.
        /// Pass name/value pairs in the order they should appear, for example
        /// Sync(this, "CourseID", courseId, "ModuleID", moduleId).
        /// </summary>
        public static void Sync(Page page, params string[] nameThenValue)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            if (nameThenValue == null || nameThenValue.Length == 0)
                throw new ArgumentException("Supply at least one name/value pair.", "nameThenValue");

            if (nameThenValue.Length % 2 != 0)
                throw new ArgumentException("Supply name/value pairs, not a dangling name.", "nameThenValue");

            var query = new List<string>();
            for (int i = 0; i < nameThenValue.Length; i += 2)
            {
                string name = nameThenValue[i];
                string value = nameThenValue[i + 1];
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                    continue;

                query.Add(HttpUtility.UrlEncode(name) + "=" + HttpUtility.UrlEncode(value));
            }

            // Request.Path excludes the query string, so this discards whatever
            // the page arrived with and rebuilds it from the new selection.
            string url = query.Count == 0
                ? page.Request.Path
                : page.Request.Path + "?" + string.Join("&", query);

            string script =
                "if (window.history && window.history.replaceState) {"
                + "window.history.replaceState(null, '', '" + HttpUtility.JavaScriptStringEncode(url) + "');"
                + "}";

            page.ClientScript.RegisterStartupScript(
                typeof(UrlSync),
                "urlsync_" + url.GetHashCode(),
                script,
                true);
        }
    }
}
