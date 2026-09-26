using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;
using TechStackLearningHub.Web.Masterpages;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class ManageUsers : Page
    {
        private readonly UserBLL _userBLL = new UserBLL();

        protected void Page_Load(object sender, EventArgs e)
        {
            // The admin shell (topbar title, highlighted sidebar link) lives in
            // Admin.Master and reads its state from these two calls, so they run
            // before the IsPostBack early-return: a postback render comes back
            // through here too, and would otherwise come up untitled.
            var master = (AdminMaster)Master;
            master.SetPageTitle("Manage Users");
            master.SetActiveNav("ManageUsers.aspx");

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
                User target = FindUser(userId);
                string username = target.Username;
                string message;
                switch (e.CommandName)
                {
                    case "ToggleActive":
                        message = ToggleActive(target, me);
                        break;
                    case "MakeAdmin":
                        _userBLL.ChangeUserRole(userId, "Admin");
                        message = "\"" + username + "\" is now an admin.";
                        break;
                    case "MakeStudent":
                        if (userId == me)
                            throw new ValidationException("You cannot demote your own account.");
                        _userBLL.ChangeUserRole(userId, "Student");
                        message = "\"" + username + "\" is now a student.";
                        break;
                    default:
                        return;
                }
                BindGrid();
                ShowSuccess(message);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        // The user comes from the BLL, not grdUsers.Rows: the grid is only bound
        // on a fresh GET, so its Rows are empty during the command's own postback.
        private string ToggleActive(User target, int me)
        {
            if (target.IsActive)
            {
                // Never let a single admin deactivate their own account and
                // lock the app out of administration.
                if (target.UserID == me)
                    throw new ValidationException("You cannot deactivate your own account.");
                _userBLL.DeactivateUser(target.UserID);
                return "Deactivated \"" + target.Username + "\".";
            }

            _userBLL.ActivateUser(target.UserID);
            return "Reactivated \"" + target.Username + "\".";
        }

        private User FindUser(int userId)
        {
            foreach (User u in _userBLL.GetAllUsersForAdmin())
            {
                if (u.UserID == userId)
                    return u;
            }

            throw new ValidationException("That account no longer exists. Refresh the list and try again.");
        }

        // ValidationException text is authored for the admin and safe to show;
        // anything else is logged rather than rendered.
        private void ShowError(Exception ex)
        {
            AdminUi.Error(lblMessage, lblSuccess, ex, "The user could not be updated. Please try again.", "ManageUsers.grdUsers_RowCommand");
        }

        private void ShowSuccess(string message)
        {
            AdminUi.Success(lblMessage, lblSuccess, message);
        }
    }
}
