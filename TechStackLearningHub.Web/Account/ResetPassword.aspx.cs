using System;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;

namespace TechStackLearningHub.Web.Account
{
    public partial class ResetPassword : Page
    {
        // Generic text for the catch-all in btnReset_Click. The only honest
        // thing to say about an unexpected fault is that it happened: the
        // visitor holds a credential and gets no detail about the row it points
        // at, the hash, or the database behind it.
        private const string GenericFailureMessage =
            "Your password could not be reset. Please try again later.";

        protected void Page_Load(object sender, EventArgs e)
        {
            // The token is read from the query string on EVERY request, this one
            // and every postback, and it is never written anywhere else. No
            // field, no ViewState key, no Session slot.
            //
            // Why it survives a postback, which is the part that decides whether
            // storing it would even be necessary: the page's <form> is emitted by
            // the Web Forms runtime with no action attribute, and a form with no
            // action posts to the URL the browser is currently on - query string
            // included. So the __VIEWSTATEPOST / __EVENTTARGET round trip lands
            // on ".../ResetPassword.aspx?token=...", the same request the ASPX
            // directive resolved this class from, and Request.QueryString is
            // already repopulated before Page_Load runs. The postback describes
            // itself; nothing has to be carried forward for it to do so.
            //
            // Why not carry it forward anyway: each of those places writes a
            // live credential into somewhere it does not have to be. ViewState
            // travels to the browser and back on every postback, and it is
            // base64, not encrypted - so the token would end up in the page
            // source, in browser history and in any cached copy of the response,
            // for the whole 30 minutes it is redeemable. A hidden field posts it
            // back in the request body and is equally readable in the markup.
            // Session would need a key derived from something the caller
            // supplied, and would keep the token alive in server state after the
            // request that needed it. The URL is where the token was delivered in
            // the first place; reading it where it arrives keeps exactly one
            // copy in flight instead of three.
            //
            // The honest cost of reading it from the URL is that query strings
            // are the one place a Referer header can leak, and this page loads
            // its assets from this site only, so there is no off-origin request
            // for it to travel to.
            string token = Request.QueryString["token"];

            // A blank or absent token is the SAME situation as a spent or expired
            // one, and gets the same single message, because "there was no token
            // at all" versus "that token was already used" is precisely the
            // distinction somebody guessing tokens wants. AuthBLL makes the same
            // fold for every way a link can fail.
            bool hasToken = !string.IsNullOrWhiteSpace(token);

            // Panels, so "not rendered" means not rendered: a control with
            // Visible=false emits no markup at all, so there is no password form
            // in the response to read, to fill in, or to post back - not merely
            // a hidden one. The form only exists on a request that carries a
            // token, and it is decided on every request, not just the first, so
            // a postback cannot be used to get a form without one.
            pnlResetForm.Visible = hasToken;
            pnlInvalidLink.Visible = !hasToken;
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            // Server-side confirmation check as well as the client validators,
            // because client validation can be bypassed.
            if (!string.Equals(txtNewPassword.Text, txtConfirmPassword.Text, StringComparison.Ordinal))
            {
                lblMessage.CssClass = "text-danger";
                lblMessage.Text = "Passwords do not match.";
                lblMessage.Visible = true;
                return;
            }

            // Re-read from the query string rather than from a field kept across
            // the postback - the same reasoning as in Page_Load, in one line.
            // It cannot be blank here: this button only exists inside a panel
            // that Page_Load showed because a token was present, and the BLL
            // null-guards anyway.
            string token = Request.QueryString["token"];

            try
            {
                // ValidatePassword is applied inside the BLL, and a rejection
                // there happens BEFORE the token is stamped, so a password the
                // user is not happy with does not spend their link.
                new AuthBLL().CompletePasswordReset(token, txtNewPassword.Text);
                Response.Redirect("~/Account/Login.aspx?reset=1");
            }
            catch (ValidationException ex)
            {
                // Authored for the user - e.g. the token is spent or expired, or
                // the account has been deactivated. The form stays rendered
                // underneath so a mistyped password can be retried against a
                // link that is still good.
                lblMessage.CssClass = "text-danger";
                lblMessage.Text = HttpUtility.HtmlEncode(ex.Message);
                lblMessage.Visible = true;
            }
            catch (ThreadAbortException)
            {
                // Response.Redirect throws ThreadAbortException to end the
                // request. Rethrow so a successful redirect is not mistaken
                // for a fault and logged.
                throw;
            }
            catch (Exception ex)
            {
                // Everything else is a fault. Logged with its context - and
                // note what is NOT logged, which is the token: the context string
                // and the exception are written, the credential is not.
                ErrorLogger.Log(ex, "ResetPassword.btnReset");
                lblMessage.CssClass = "text-danger";
                lblMessage.Text = GenericFailureMessage;
                lblMessage.Visible = true;
            }
        }
    }
}
