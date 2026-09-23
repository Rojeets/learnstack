using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;

namespace TechStackLearningHub.Web
{
    public partial class Register : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            if (Request.QueryString["registered"] == "1")
            {
                lblError.CssClass = "text-success";
                lblError.Text = "Account created. You can now log in.";
                lblError.Visible = true;
            }
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            // Server-side confirmation check as well as the client validators,
            // because client validation can be bypassed.
            if (!string.Equals(txtPassword.Text, txtConfirmPassword.Text, StringComparison.Ordinal))
            {
                lblError.CssClass = "text-danger";
                lblError.Text = "Passwords do not match.";
                lblError.Visible = true;
                return;
            }

            try
            {
                new AuthService().Register(txtUsername.Text.Trim(), txtEmail.Text.Trim(), txtPassword.Text);
                Response.Redirect("~/Login.aspx?registered=1");
            }
            catch (InvalidOperationException ex)
            {
                lblError.CssClass = "text-danger";
                lblError.Text = ex.Message;
                lblError.Visible = true;
            }
        }
    }
}