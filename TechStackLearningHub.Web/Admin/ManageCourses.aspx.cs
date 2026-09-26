using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class ManageCourses : Page
    {
        private readonly CourseBLL _courseBLL = new CourseBLL();

        protected void Page_Load(object sender, EventArgs e)
        {

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
                if (int.TryParse(hidCourseId.Value, out courseId) && courseId > 0)
                {
                    Course course = _courseBLL.GetCourseForAdminEdit(courseId);
                    course.CourseName = txtCourseName.Text.Trim();
                    course.TechStack = ddlTechStack.SelectedValue;
                    course.Description = txtDescription.Text.Trim();
                    _courseBLL.UpdateCourse(course);
                }
                else
                {
                    _courseBLL.CreateCourse(new Course
                    {
                        CourseName = txtCourseName.Text.Trim(),
                        TechStack = ddlTechStack.SelectedValue,
                        Description = txtDescription.Text.Trim(),
                        IsPublished = false
                    });
                }

                ResetEditor(null);
                BindGrid();
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
                            _courseBLL.PublishCourse(courseId);
                        else
                            _courseBLL.UnpublishCourse(courseId);
                        break;
                    case "DeleteCourse":
                        _courseBLL.DeleteCourse(courseId);
                        break;
                    case "ManageModules":
                        Response.Redirect("ManageModules.aspx?CourseID=" + courseId);
                        break;
                }
                BindGrid();
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
        }

        // A ValidationException is authored for the admin, so its text is safe
        // to render. Every other exception is a fault: it is logged, and the
        // admin gets a fixed message instead of whatever the exception said.
        // The previous code caught InvalidOperationException, which the BLL no
        // longer throws - validation failures would have escaped as unhandled
        // error pages instead of inline messages.
        private void ShowError(Exception ex, string context)
        {
            lblMessage.CssClass = "text-danger d-block mt-2";
            var validation = ex as ValidationException;
            if (validation != null)
            {
                lblMessage.Text = HttpUtility.HtmlEncode(validation.Message);
            }
            else
            {
                ErrorLogger.Log(ex, context);
                lblMessage.Text = "The course could not be saved. Please try again.";
            }
            lblMessage.Visible = true;
        }
    }
}
