using System;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;

namespace TechStackLearningHub.Web.Account
{
    public partial class ForgotPassword : Page
    {
        // The single sentence this page shows for every accepted submit, known
        // or unknown address alike.
        //
        // A const, not an inline literal, because the enumeration-resistance of
        // the feature depends on there being exactly ONE of them: any wording
        // that branched on the outcome - "no account for that address", "sent",
        // "we are not telling you" - would turn this form into a free oracle for
        // testing whether somebody has an account here. The message is therefore
        // phrased so that it is true for both cases and false for neither, and
        // it states the expiry so the user knows how long they have without
        // having to be told whether anything was actually sent.
        private const string NeutralConfirmationMessage =
            "If that address has an account, a reset link is on its way. It expires in 30 minutes.";

        // Generic text for the catch-all below. Says nothing about addresses,
        // accounts, tokens or the database, because this handler cannot tell the
        // visitor anything they are entitled to know: a fault here is just as
        // likely to have happened on a request for a real account as a fake one.
        private const string GenericFailureMessage =
            "We could not process your request. Please try again later.";

        protected void btnSendLink_Click(object sender, EventArgs e)
        {
            try
            {
                // Returns normally for an unknown address, a deactivated account
                // and a mail server that is down - the BLL swallows and logs SMTP
                // failures on purpose, because surfacing them here would tell a
                // visitor that their address IS registered, which is the exact
                // oracle this page must not become. So the page must not go
                // looking for the difference either: no second query, no timing
                // probe, no different output.
                new AuthBLL().RequestPasswordReset(txtEmail.Text.Trim());

                // The address is in a mail flow now. Leaving it in the box would
                // redisplay it after the postback - on screen, and in the
                // rendered HTML of the response - for no benefit to anyone.
                txtEmail.Text = string.Empty;

                lblMessage.CssClass = "text-success";
                lblMessage.Text = NeutralConfirmationMessage;
                lblMessage.Visible = true;

                // Deliberately no redirect and no disabled button. The neutral
                // message has to arrive on THIS request, because a page that
                // behaved one way for a real address and another way for a fake
                // one is the leak, and a redirect would replace the one answer
                // this page gives with a second round trip to compare.
            }
            catch (ValidationException ex)
            {
                // Authored for the user - e.g. the reset could not be recorded
                // because the database rejected the write.
                lblMessage.CssClass = "text-danger";
                lblMessage.Text = HttpUtility.HtmlEncode(ex.Message);
                lblMessage.Visible = true;
            }
            catch (ThreadAbortException)
            {
                // Nothing on this page redirects, but ThreadAbortException is how
                // the runtime ends a request that was ended by something else -
                // the master page, a module, Response.End. Rethrow so a
                // deliberate end of request is not mistaken for a fault and
                // logged.
                throw;
            }
            catch (Exception ex)
            {
                // Everything else is a fault. Logged with its context and
                // replaced with text that reveals nothing: no address, no
                // account state, no SQL.
                ErrorLogger.Log(ex, "ForgotPassword.btnSendLink");
                lblMessage.CssClass = "text-danger";
                lblMessage.Text = GenericFailureMessage;
                lblMessage.Visible = true;
            }
        }
    }
}
