using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;
using TechStackLearningHub.Helpers;

namespace TechStackLearningHub.Web.Admin
{
    public partial class ManageUsers : Page
    {
        private readonly UserBLL _userBLL = new UserBLL();

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
                BindGrid();
        }

        private void BindGrid()
        {
            grdUsers.DataSource = _userBLL.GetAllUsersForAdmin();
            grdUsers.DataBind();
        }

        protected void grdUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            AuthBLL.RequireAdmin();

            int userId;
            if (!int.TryParse(e.CommandArgument as string, out userId))
                return;

            try
            {
                int me = AuthBLL.CurrentUserId;
                switch (e.CommandName)
                {
                    case "ToggleActive":
                        ToggleActive(userId, me);
                        break;
                    case "MakeAdmin":
                        _userBLL.ChangeUserRole(userId, "Admin");
                        break;
                    case "MakeStudent":
                        if (userId == me)
                            throw new ValidationException("You cannot demote your own account.");
                        _userBLL.ChangeUserRole(userId, "Student");
                        break;
                }
                lblMessage.Visible = false;
                BindGrid();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void ToggleActive(int userId, int me)
        {
            foreach (GridViewRow row in grdUsers.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                    continue;
                if (Convert.ToInt32(grdUsers.DataKeys[row.RowIndex].Value) != userId)
                    continue;

                bool isActive = (bool)DataBinder.Eval(row.DataItem, "IsActive");
                if (isActive)
                {
                    // Never let a single admin deactivate their own account and
                    // lock the app out of administration.
                    if (userId == me)
                        throw new ValidationException("You cannot deactivate your own account.");
                    _userBLL.DeactivateUser(userId);
                }
                else
                {
                    _userBLL.ActivateUser(userId);
                }
                return;
            }
        }

        // ValidationException text is authored for the admin and safe to show;
        // anything else is logged rather than rendered.
        private void ShowError(Exception ex)
        {
            var validation = ex as ValidationException;
            if (validation != null)
            {
                lblMessage.Text = HttpUtility.HtmlEncode(validation.Message);
            }
            else
            {
                ErrorLogger.Log(ex, "ManageUsers.grdUsers_RowCommand");
                lblMessage.Text = "The user could not be updated. Please try again.";
            }
            lblMessage.Visible = true;
        }
    }
}
