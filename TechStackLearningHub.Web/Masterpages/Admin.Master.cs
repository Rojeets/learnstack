using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;

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
        }

        protected void btnAdminLogout_Click(object sender, EventArgs e)
        {
            new AuthBLL().Logout();
            Response.Redirect("~/Account/Login.aspx");
        }
    }
}
