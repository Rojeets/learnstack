using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;

namespace TechStackLearningHub.Web.Account
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
                new AuthBLL().Register(txtUsername.Text.Trim(), txtEmail.Text.Trim(), txtPassword.Text);
                Response.Redirect("~/Account/Login.aspx?registered=1");
            }
            catch (ValidationException ex)
            {
                // Authored for the user - e.g. the username is already taken.
                lblError.CssClass = "text-danger";
                lblError.Text = HttpUtility.HtmlEncode(ex.Message);
                lblError.Visible = true;
            }
            catch (Exception ex)
            {
                // Everything else is a fault. It used to be caught and echoed
                // to the page because the catch clause was typed to
                // InvalidOperationException, which also swallowed
                // "The Student role is not configured" - a configuration fault
                // whose text has no business on a public page.
                ErrorLogger.Log(ex, "Register.btnRegister");
                lblError.CssClass = "text-danger";
                lblError.Text = "Your account could not be created. Please try again later.";
                lblError.Visible = true;
            }
        }
    }
}