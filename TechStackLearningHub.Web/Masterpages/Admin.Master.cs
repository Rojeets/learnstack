using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;

namespace TechStackLearningHub.Web.Masterpages
{
    /// <summary>
    /// Chrome for every /Admin page. Picking this master is all a new admin
    /// page has to do to inherit the role gate.
    ///
    /// WHY THE GATE LIVES IN Page_Init AND NOT Page_Load
    /// --------------------------------------------------
    /// WebForms postback order for a master-integrated page is:
    ///
    ///     Page.Init  ->  Master.Init  ->  RaisePostBackEvent  ->  Page.Load  ->  Master.Load
    ///                                     (btnClick handlers run HERE)
    ///
    /// RaisePostBackEvent fires BEFORE Page.Load. A check written in
    /// Page_Load therefore runs after btnClick_Click has already executed, so
    /// a student POSTing to an admin page would have its write committed and
    /// only then be redirected - the response body is discarded, the row is
    /// not. Page_Init runs before RaisePostBackEvent, so it actually blocks.
    ///
    /// This is why the old App_Start/RoleGuard.cs checks (all called from
    /// Page_Load) never protected write handlers. Do not "simplify" this back
    /// into Page_Load.
    ///
    /// Defence in depth: Web.config additionally denies anonymous users on the
    /// Admin folder at the IIS pipeline level, and every admin write handler
    /// also calls AuthBLL.RequireAdmin() as its first statement.
    /// </summary>
    public partial class AdminMaster : System.Web.UI.MasterPage
    {
        // Sidebar link ids, in markup order. SetActiveNav walks this list to
        // clear the current highlight before marking the incoming page, so a
        // new destination is one <li> here and one nav-link id, not a
        // per-page hardcoded class.
        private static readonly string[] NavLinkIds =
        {
            "navDashboard", "navCourses", "navModules", "navLessons",
            "navQuizzes", "navQuestions", "navUsers", "navReports"
        };

        private string _pageTitle;
        private string _activeNavId;

        /// <summary>
        /// Called from each admin page's Page_Load, before its IsPostBack
        /// early-return, so the shell is right on postback renders too.
        /// </summary>
        public void SetPageTitle(string title)
        {
            _pageTitle = title;
        }

        /// <summary>
        /// PageFileName of the calling page ("ManageCourses.aspx"). Matched
        /// against the link hrefs so the master stays generic. GetFileName
        /// because NavigateUrl is app-relative ("~/Admin/ManageCourses.aspx")
        /// while the caller only knows its own file name.
        /// </summary>
        public void SetActiveNav(string pageFileName)
        {
            for (int i = 0; i < NavLinkIds.Length; i++)
            {
                var link = FindControl(NavLinkIds[i]) as HyperLink;
                if (link == null)
                    continue;

                string hrefFileName = Path.GetFileName(link.NavigateUrl);
                if (string.Equals(hrefFileName, pageFileName, StringComparison.OrdinalIgnoreCase))
                    _activeNavId = NavLinkIds[i];
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // Response.Redirect ends the request, so the two calls below are
            // effectively early returns; written this way for readability.
            if (!AuthBLL.IsLoggedIn)
            {
                AuthBLL.RedirectToLogin();
                return;
            }

            if (!AuthBLL.IsAdmin)
            {
                Response.Redirect("~/Pages/Default.aspx");
                return;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // HttpUtility.HtmlEncode even though the name came from our own
            // database - defence in depth against a crafted display name.
            litAdminName.Text = HttpUtility.HtmlEncode(AuthBLL.CurrentUserName);

            litPageTitle.Text = HttpUtility.HtmlEncode(_pageTitle ?? string.Empty);

            for (int i = 0; i < NavLinkIds.Length; i++)
            {
                HyperLink link = FindControl(NavLinkIds[i]) as HyperLink;
                if (link == null)
                    continue;

                string css = "nav-link" + (NavLinkIds[i] == _activeNavId ? " active" : string.Empty);
                link.CssClass = css;
            }
        }

        protected void btnAdminLogout_Click(object sender, EventArgs e)
        {
            new AuthBLL().Logout();
            Response.Redirect("~/Account/Login.aspx");
        }
    }
}
