using System;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;
using TechStackLearningHub.Web.Masterpages;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class ManageCourses : Page
    {
        private readonly CourseBLL _courseBLL = new CourseBLL();

        protected void Page_Load(object sender, EventArgs e)
        {
            // The topbar title and the sidebar highlight are rendered by the
            // master in Master.Load, which runs after this handler - so they
            // have to be set on every request, before the IsPostBack guard.
            // Skipping them on postback would blank the title on every save,
            // delete and Edit/Publish click.
            var master = (AdminMaster)Master;
            master.SetPageTitle("Manage Courses");
            master.SetActiveNav("ManageCourses.aspx");

            if (!IsPostBack)
            {
                foreach (string stack in CourseBLL.KnownTechStacks)
                {
                    ddlTechStack.Items.Add(new ListItem(stack, stack));
                }
                BindGrid();
            }
        }

        private void BindGrid()
        {
            grdCourses.DataSource = _courseBLL.GetAllCoursesForAdmin();
            grdCourses.DataBind();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            AuthBLL.RequireAdmin();

            try
            {
                int courseId;
                string savedName;
                if (int.TryParse(hidCourseId.Value, out courseId) && courseId > 0)
                {
                    Course course = _courseBLL.GetCourseForAdminEdit(courseId);
                    course.CourseName = txtCourseName.Text.Trim();
                    course.TechStack = ddlTechStack.SelectedValue;
                    course.Description = txtDescription.Text.Trim();
                    _courseBLL.UpdateCourse(course);
                    savedName = course.CourseName;
                }
                else
                {
                    savedName = txtCourseName.Text.Trim();
                    _courseBLL.CreateCourse(new Course
                    {
                        CourseName = savedName,
                        TechStack = ddlTechStack.SelectedValue,
                        Description = txtDescription.Text.Trim(),
                        IsPublished = false
                    });
                }

                ResetEditor(null);
                BindGrid();
                ShowSuccess("Saved \"" + savedName + "\".");
            }
            catch (Exception ex)
            {
                ShowError(ex, "ManageCourses.btnSave");
            }
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ResetEditor(null);
        }

        protected void grdCourses_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            AuthBLL.RequireAdmin();

            int courseId;
            if (!int.TryParse(e.CommandArgument as string, out courseId))
                return;

            try
            {
                switch (e.CommandName)
                {
                    case "EditCourse":
                        StartEdit(courseId);
                        break;
                    case "TogglePublish":
                        Course course = _courseBLL.GetCourseForAdminEdit(courseId);
                        if (course == null)
                            return;
                        if (!course.IsPublished)
                        {
                            _courseBLL.PublishCourse(courseId);
                            ShowSuccess("Published \"" + course.CourseName + "\".");
                        }
                        else
                        {
                            _courseBLL.UnpublishCourse(courseId);
                            ShowSuccess("Unpublished \"" + course.CourseName + "\". It is hidden from students again.");
                        }
                        break;
                    case "DeleteCourse":
                        Course deleted = _courseBLL.GetCourseForAdminEdit(courseId);
                        _courseBLL.DeleteCourse(courseId);
                        ShowSuccess("Deleted \"" + (deleted == null ? "the course" : deleted.CourseName) + "\".");
                        break;
                    case "ManageModules":
                        Response.Redirect("ManageModules.aspx?CourseID=" + courseId);
                        break;
                }
                BindGrid();
            }
            catch (ThreadAbortException)
            {
                // The ManageModules command redirects out; that is a success.
                throw;
            }
            catch (Exception ex)
            {
                ShowError(ex, "ManageCourses.grdCourses_RowCommand");
                BindGrid();
            }
        }

        private void StartEdit(int courseId)
        {
            Course course = _courseBLL.GetCourseForAdminEdit(courseId);
            if (course == null)
                return;

            hidCourseId.Value = courseId.ToString();
            txtCourseName.Text = course.CourseName;
            if (ddlTechStack.Items.FindByValue(course.TechStack) != null)
                ddlTechStack.SelectedValue = course.TechStack;
            txtDescription.Text = course.Description;
            lblEditorHeading.InnerText = "Edit course";
            btnCancelEdit.Visible = true;
        }

        private void ResetEditor(object _)
        {
            hidCourseId.Value = "";
            txtCourseName.Text = "";
            txtDescription.Text = "";
            if (ddlTechStack.Items.Count > 0)
                ddlTechStack.SelectedIndex = 0;
            lblEditorHeading.InnerText = "New course";
            btnCancelEdit.Visible = false;
            lblMessage.Visible = false;
            lblSuccess.Visible = false;
        }

        private void ShowSuccess(string message)
        {
            AdminUi.Success(lblMessage, lblSuccess, message);
        }

        // A ValidationException is authored for the admin, so its text is safe
        // to render. Every other exception is a fault: it is logged, and the
        // admin gets a fixed message instead of whatever the exception said.
        // The previous code caught InvalidOperationException, which the BLL no
        // longer throws - validation failures would have escaped as unhandled
        // error pages instead of inline messages.
        private void ShowError(Exception ex, string context)
        {
            AdminUi.Error(lblMessage, lblSuccess, ex, "The course could not be saved. Please try again.", context);
        }
    }
}
