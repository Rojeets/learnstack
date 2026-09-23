using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;

namespace TechStackLearningHub.Web.Admin
{
    public partial class ManageModules : Page
    {
        private readonly CourseService _courseService = new CourseService();
        private readonly ModuleService _moduleService = new ModuleService();

        protected void Page_Load(object sender, EventArgs e)
        {
            RoleGuard.RequireAdmin(this);

            if (!IsPostBack)
            {
                BindCourseSelector();
                int requestedCourse;
                if (int.TryParse(Request.QueryString["CourseID"], out requestedCourse))
                {
                    ListItem item = ddlCourse.Items.FindByValue(requestedCourse.ToString());
                    if (item != null)
                        ddlCourse.SelectedValue = requestedCourse.ToString();
                }
                LoadWorkspace();
            }
        }

        private void BindCourseSelector()
        {
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new ListItem("-- choose a course --", ""));
            foreach (DataRow row in _courseService.GetAllCoursesForAdmin().Rows)
            {
                ddlCourse.Items.Add(new ListItem(
                    string.Format("{0} ({1})", row["CourseName"], row["TechStack"]),
                    row["CourseID"].ToString()));
            }
        }

        protected void ddlCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlRename.Visible = false;
            LoadWorkspace();
        }

        private int CurrentCourseId
        {
            get
            {
                int courseId;
                return int.TryParse(ddlCourse.SelectedValue, out courseId) ? courseId : 0;
            }
        }

        private void LoadWorkspace()
        {
            if (CurrentCourseId <= 0)
            {
                pnlWorkspace.Visible = false;
                return;
            }

            pnlWorkspace.Visible = true;
            grdModules.DataSource = _moduleService.GetModulesForCourse(CurrentCourseId);
            grdModules.DataBind();
        }

        protected void btnAddModule_Click(object sender, EventArgs e)
        {
            try
            {
                _moduleService.AddModuleToCourse(CurrentCourseId, txtModuleTitle.Text.Trim());
                txtModuleTitle.Text = "";
                ClearMessage();
                LoadWorkspace();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        protected void grdModules_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int moduleId;
            if (!int.TryParse(e.CommandArgument as string, out moduleId))
                return;

            try
            {
                switch (e.CommandName)
                {
                    case "RenameModule":
                        BeginRename(moduleId);
                        break;
                    case "DeleteModule":
                        _moduleService.DeleteModule(moduleId);
                        pnlRename.Visible = false;
                        break;
                    case "ManageLessons":
                        Response.Redirect("ManageLessons.aspx?ModuleID=" + moduleId);
                        break;
                    case "MoveUp":
                    case "MoveDown":
                        Reorder(moduleId, e.CommandName == "MoveUp");
                        break;
                }
                ClearMessage();
                LoadWorkspace();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void BeginRename(int moduleId)
        {
            var module = _moduleService.GetModuleById(moduleId);
            if (module == null)
                return;
            hidRenameModuleId.Value = moduleId.ToString();
            txtRenameTitle.Text = module.ModuleTitle;
            pnlRename.Visible = true;
        }

        protected void btnSaveRename_Click(object sender, EventArgs e)
        {
            try
            {
                int moduleId;
                if (int.TryParse(hidRenameModuleId.Value, out moduleId) && moduleId > 0)
                {
                    _moduleService.RenameModule(moduleId, txtRenameTitle.Text.Trim());
                    pnlRename.Visible = false;
                    ClearMessage();
                    LoadWorkspace();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        protected void btnCancelRename_Click(object sender, EventArgs e)
        {
            pnlRename.Visible = false;
        }

        private void Reorder(int moduleId, bool moveUp)
        {
            DataTable modules = _moduleService.GetModulesForCourse(CurrentCourseId);
            var ordered = new List<int>();
            foreach (DataRow row in modules.Rows)
                ordered.Add((int)row["ModuleID"]);

            int index = ordered.IndexOf(moduleId);
            int target = moveUp ? index - 1 : index + 1;
            if (index < 0 || target < 0 || target >= ordered.Count)
                return;

            ordered.RemoveAt(index);
            ordered.Insert(target, moduleId);
            _moduleService.ReorderModules(ordered);
        }

        private void ClearMessage()
        {
            lblMessage.Visible = false;
        }

        private void ShowError(string message)
        {
            lblMessage.Text = System.Web.HttpUtility.HtmlEncode(message);
            lblMessage.Visible = true;
        }
    }
}