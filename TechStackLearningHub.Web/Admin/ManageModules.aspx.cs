using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class ManageModules : Page
    {
        private readonly CourseBLL _courseBLL = new CourseBLL();
        private readonly ModuleBLL _moduleBLL = new ModuleBLL();

        protected void Page_Load(object sender, EventArgs e)
        {

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
            foreach (Course course in _courseBLL.GetAllCoursesForAdmin())
            {
                ddlCourse.Items.Add(new ListItem(
                    string.Format("{0} ({1})", course.CourseName, course.TechStack),
                    course.CourseID.ToString()));
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
            grdModules.DataSource = _moduleBLL.GetModulesForCourse(CurrentCourseId);
            grdModules.DataBind();
        }

        protected void btnAddModule_Click(object sender, EventArgs e)
        {
            AuthBLL.RequireAdmin();

            try
            {
                _moduleBLL.AddModuleToCourse(CurrentCourseId, txtModuleTitle.Text.Trim());
                txtModuleTitle.Text = "";
                ClearMessage();
                LoadWorkspace();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        protected void grdModules_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            AuthBLL.RequireAdmin();

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
                        _moduleBLL.DeleteModule(moduleId);
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
            catch (ThreadAbortException)
            {
                // The ManageLessons command redirects out; that is a success.
                throw;
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void BeginRename(int moduleId)
        {
            var module = _moduleBLL.GetModuleById(moduleId);
            if (module == null)
                return;
            hidRenameModuleId.Value = moduleId.ToString();
            txtRenameTitle.Text = module.ModuleTitle;
            pnlRename.Visible = true;
        }

        protected void btnSaveRename_Click(object sender, EventArgs e)
        {
            AuthBLL.RequireAdmin();

            try
            {
                int moduleId;
                if (int.TryParse(hidRenameModuleId.Value, out moduleId) && moduleId > 0)
                {
                    _moduleBLL.RenameModule(moduleId, txtRenameTitle.Text.Trim());
                    pnlRename.Visible = false;
                    ClearMessage();
                    LoadWorkspace();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        protected void btnCancelRename_Click(object sender, EventArgs e)
        {
            pnlRename.Visible = false;
        }

        private void Reorder(int moduleId, bool moveUp)
        {
            List<Module> modules = _moduleBLL.GetModulesForCourse(CurrentCourseId);
            var ordered = new List<int>();
            foreach (Module module in modules)
                ordered.Add(module.ModuleID);

            int index = ordered.IndexOf(moduleId);
            int target = moveUp ? index - 1 : index + 1;
            if (index < 0 || target < 0 || target >= ordered.Count)
                return;

            ordered.RemoveAt(index);
            ordered.Insert(target, moduleId);
            _moduleBLL.ReorderModules(ordered);
        }

        private void ClearMessage()
        {
            lblMessage.Visible = false;
        }

        // Single place that decides what the admin is allowed to read. A
        // ValidationException carries an authored, user-facing message; every
        // other exception is a fault whose text could contain a connection
        // string or SQL, so it is logged and replaced.
        private void ShowError(Exception ex)
        {
            var validation = ex as ValidationException;
            if (validation != null)
            {
                lblMessage.Text = System.Web.HttpUtility.HtmlEncode(validation.Message);
            }
            else
            {
                ErrorLogger.Log(ex, "ManageModules");
                lblMessage.Text = "The module could not be saved. Please try again.";
            }
            lblMessage.Visible = true;
        }
    }
}
