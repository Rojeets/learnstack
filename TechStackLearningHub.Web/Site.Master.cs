using System;
using System.Web;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;

namespace TechStackLearningHub.Web
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string roleName = Session["RoleName"] as string;
            bool authenticated = !string.IsNullOrEmpty(roleName);

            pnlAnonymous.Visible = !authenticated;
            pnlStudent.Visible = authenticated && roleName == "Student";
            pnlAdmin.Visible = authenticated && roleName == "Admin";
            pnlAuthenticated.Visible = authenticated;

            if (authenticated)
            {
                litUser.Text = HttpUtility.HtmlEncode("Signed in as " + Session["Username"]);
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            new AuthService().Logout();
            Response.Redirect("~/Login.aspx");
        }
    }
}