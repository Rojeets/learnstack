using System.Web.UI;

namespace TechStackLearningHub.Web
{
    // Centralised authorisation helper used by role-restricted pages. The
    // <location> rules in Web.config stop anonymous users; this double-checks
    // the Session so a logged-in student can never browse an Admin URL (and
    // vice-versa), even via a hand-typed address.
    public static class RoleGuard
    {
        public static bool IsAdmin(Page page)
        {
            return page.Session["RoleName"] as string == "Admin";
        }

        public static bool IsStudent(Page page)
        {
            return page.Session["RoleName"] as string == "Student";
        }

        public static void RequireAdmin(Page page)
        {
            if (IsAdmin(page))
                return;
            RedirectAway(page);
        }

        public static void RequireStudent(Page page)
        {
            if (IsStudent(page))
                return;
            RedirectAway(page);
        }

        public static void RequireLoggedIn(Page page)
        {
            if (page.Session["UserID"] != null)
                return;
            page.Response.Redirect("~/Login.aspx");
        }

        private static void RedirectAway(Page page)
        {
            if (page.Session["UserID"] == null)
                page.Response.Redirect("~/Login.aspx");
            else
                page.Response.Redirect("~/Student/Dashboard.aspx");
        }
    }
}