using System;
using System.Web;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;

namespace TechStackLearningHub.Web.Masterpages
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // All session reads go through AuthBLL so "how do we know who is
            // signed in" lives in exactly one class.
            string roleName = AuthBLL.CurrentRoleName;
            bool authenticated = AuthBLL.IsLoggedIn;

            pnlAnonymous.Visible = !authenticated;
            pnlStudent.Visible = authenticated && roleName == "Student";
            pnlAdmin.Visible = authenticated && roleName == "Admin";
            pnlAuthenticated.Visible = authenticated;

            if (authenticated)
            {
                litUser.Text = HttpUtility.HtmlEncode("Signed in as " + AuthBLL.CurrentUserName);
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            new AuthBLL().Logout();
            Response.Redirect("~/Account/Login.aspx");
        }
    }
}