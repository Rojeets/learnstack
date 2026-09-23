using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;

namespace TechStackLearningHub.Web.Admin
{
    public partial class ManageUsers : Page
    {
        private readonly UserService _userService = new UserService();

        protected void Page_Load(object sender, EventArgs e)
        {
            RoleGuard.RequireAdmin(this);

            if (!IsPostBack)
                BindGrid();
        }

        private void BindGrid()
        {
            grdUsers.DataSource = _userService.GetAllUsersForAdmin();
            grdUsers.DataBind();
        }

        protected void grdUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int userId;
            if (!int.TryParse(e.CommandArgument as string, out userId))
                return;

            try
            {
                int me = (int)Session["UserID"];
                switch (e.CommandName)
                {
                    case "ToggleActive":
                        ToggleActive(userId, me);
                        break;
                    case "MakeAdmin":
                        _userService.ChangeUserRole(userId, "Admin");
                        break;
                    case "MakeStudent":
                        if (userId == me)
                            throw new InvalidOperationException("You cannot demote your own account.");
                        _userService.ChangeUserRole(userId, "Student");
                        break;
                }
                lblMessage.Visible = false;
                BindGrid();
            }
            catch (Exception ex)
            {
                lblMessage.Text = HttpUtility.HtmlEncode(ex.Message);
                lblMessage.Visible = true;
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
                        throw new InvalidOperationException("You cannot deactivate your own account.");
                    _userService.DeactivateUser(userId);
                }
                else
                {
                    _userService.ActivateUser(userId);
                }
                return;
            }
        }
    }
}