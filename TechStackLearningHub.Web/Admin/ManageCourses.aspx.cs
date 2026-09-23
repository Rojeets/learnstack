using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class ManageCourses : Page
    {
        private readonly CourseService _courseService = new CourseService();

        protected void Page_Load(object sender, EventArgs e)
        {
            RoleGuard.RequireAdmin(this);

            if (!IsPostBack)
            {
                foreach (string stack in CourseService.KnownTechStacks)
                {
                    ddlTechStack.Items.Add(new ListItem(stack, stack));
                }
                BindGrid();
            }
        }

        private void BindGrid()
        {
            grdCourses.DataSource = _courseService.GetAllCoursesForAdmin();
            grdCourses.DataBind();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                int courseId;
                if (int.TryParse(hidCourseId.Value, out courseId) && courseId > 0)
                {
                    Course course = _courseService.GetCourseForAdminEdit(courseId);
                    course.CourseName = txtCourseName.Text.Trim();
                    course.TechStack = ddlTechStack.SelectedValue;
                    course.Description = txtDescription.Text.Trim();
                    _courseService.UpdateCourse(course);
                }
                else
                {
                    _courseService.CreateCourse(new Course
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
            catch (InvalidOperationException ex)
            {
                ShowError(ex.Message);
            }
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ResetEditor(null);
        }

        protected void grdCourses_RowCommand(object sender, GridViewCommandEventArgs e)
        {
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
                        Course course = _courseService.GetCourseForAdminEdit(courseId);
                        if (course == null)
                            return;
                        if (!course.IsPublished)
                            _courseService.PublishCourse(courseId);
                        else
                            _courseService.UnpublishCourse(courseId);
                        break;
                    case "DeleteCourse":
                        _courseService.DeleteCourse(courseId);
                        break;
                    case "ManageModules":
                        Response.Redirect("ManageModules.aspx?CourseID=" + courseId);
                        break;
                }
                BindGrid();
            }
            catch (InvalidOperationException ex)
            {
                ShowError(ex.Message);
                BindGrid();
            }
        }

        private void StartEdit(int courseId)
        {
            Course course = _courseService.GetCourseForAdminEdit(courseId);
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

        private void ShowError(string message)
        {
            lblMessage.CssClass = "text-danger d-block mt-2";
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.Visible = true;
        }
    }
}