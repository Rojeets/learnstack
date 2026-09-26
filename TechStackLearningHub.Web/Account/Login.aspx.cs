using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;

namespace TechStackLearningHub.Web.Account
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            // ResetPassword.aspx lands here with ?reset=1 once a token has been
            // redeemed, so the user is told the password they were just reset to
            // is the one in front of them. Same pattern as the ?registered=1
            // block in Register.aspx.cs: first request only, and the CssClass is
            // swapped so the same label can carry a success instead of a failure.
            if (Request.QueryString["reset"] == "1")
            {
                lblError.CssClass = "text-success";
                lblError.Text = "Your password has been reset. You can now log in with your new password.";
                lblError.Visible = true;
            }

            string roleName = AuthBLL.CurrentRoleName;
            if (!string.IsNullOrEmpty(roleName))
            {
                RedirectToRoleDashboard(roleName);
                return;
            }

            // A valid Forms auth ticket with no matching session (e.g. the
            // session was recycled after a timeout) would otherwise bounce
            // between this page and the role dashboards forever. Drop the stale
            // ticket and show the login form again.
            if (Request.IsAuthenticated)
                FormsAuthentication.SignOut();
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            var auth = new AuthBLL();
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
                Response.Redirect("~/Member/Dashboard.aspx");
        }
    }
}