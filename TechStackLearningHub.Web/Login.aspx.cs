using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;

namespace TechStackLearningHub.Web
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string roleName = Session["RoleName"] as string;
            if (!string.IsNullOrEmpty(roleName))
            {
                RedirectToRoleDashboard(roleName);
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            var auth = new AuthService();
            LoginResult result = auth.Login(txtUsername.Text.Trim(), txtPassword.Text);

            if (result.Success)
            {
                auth.CreateSessionForUser(result.User);
                RedirectToRoleDashboard(result.User.RoleName);
            }
            else if (result.Deactivated)
            {
                lblError.Text = "This account has been deactivated. Contact an administrator.";
                lblError.Visible = true;
            }
            else
            {
                // Deliberately ambiguous: do not reveal whether the username
                // or the password was wrong (prevents username enumeration).
                lblError.Text = "Invalid username or password.";
                lblError.Visible = true;
            }
        }

        private void RedirectToRoleDashboard(string roleName)
        {
            if (roleName == "Admin")
                Response.Redirect("~/Admin/AdminDashboard.aspx");
            else
                Response.Redirect("~/Student/Dashboard.aspx");
        }
    }
}