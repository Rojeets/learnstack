using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class ManageQuizzes : Page
    {
        private readonly CourseBLL _courseBLL = new CourseBLL();
        private readonly ModuleBLL _moduleBLL = new ModuleBLL();
        private readonly QuizBLL _quizBLL = new QuizBLL();

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                BindCourseSelector();
                int courseId;
                if (int.TryParse(Request.QueryString["CourseID"], out courseId))
                {
                    ListItem item = ddlCourse.Items.FindByValue(courseId.ToString());
                    if (item != null)
                        ddlCourse.SelectedValue = courseId.ToString();
                }
                BindModuleOptions();
                int moduleId;
                if (int.TryParse(Request.QueryString["ModuleID"], out moduleId))
                {
                    ListItem moduleItem = ddlModule.Items.FindByValue(moduleId.ToString());
                    if (moduleItem != null)
                        ddlModule.SelectedValue = moduleId.ToString();
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

        private void BindModuleOptions()
        {
            ddlModule.Items.Clear();
            ddlModule.Items.Add(new ListItem("-- choose a module --", ""));
            int courseId;
            if (int.TryParse(ddlCourse.SelectedValue, out courseId) && courseId > 0)
            {
                foreach (Module module in _moduleBLL.GetModulesForCourse(courseId))
                {
                    ddlModule.Items.Add(new ListItem(module.ModuleTitle, module.ModuleID.ToString()));
                }
            }
        }

        protected void ddlCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindModuleOptions();
            LoadWorkspace();
        }

        protected void ddlModule_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadWorkspace();
        }

        private int CurrentModuleId
        {
            get
            {
                int moduleId;
                return int.TryParse(ddlModule.SelectedValue, out moduleId) ? moduleId : 0;
            }
        }

        private void LoadWorkspace()
        {
            if (CurrentModuleId <= 0)
            {
                pnlWorkspace.Visible = false;
                return;
            }

            pnlWorkspace.Visible = true;
            Quiz quiz = _quizBLL.GetQuizForModuleId(CurrentModuleId);
            lnkManageQuestions.NavigateUrl = quiz == null
                ? "#"
                : HttpUtility.HtmlAttributeEncode("ManageQuestions.aspx?QuizID=" + quiz.QuizID);
            lnkManageQuestions.Enabled = quiz != null;

            if (quiz == null)
            {
                hidQuizId.Value = "";
                txtQuizTitle.Text = "";
                txtPassMark.Text = "50";
                litHeading.Text = "Create quiz for this module";
            }
            else
            {
                hidQuizId.Value = quiz.QuizID.ToString();
                txtQuizTitle.Text = quiz.QuizTitle;
                txtPassMark.Text = quiz.PassMarkPercent.ToString();
                litHeading.Text = "Module quiz";
            }
            lblMessage.Visible = false;
        }

        protected void btnSaveQuiz_Click(object sender, EventArgs e)
        {
            AuthBLL.RequireAdmin();

            try
            {
                int passMark = int.Parse(txtPassMark.Text.Trim());
                int quizId;
                if (int.TryParse(hidQuizId.Value, out quizId) && quizId > 0)
                    _quizBLL.UpdateQuiz(quizId, txtQuizTitle.Text.Trim(), passMark);
                else
                    _quizBLL.CreateQuiz(CurrentModuleId, txtQuizTitle.Text.Trim(), passMark);
                LoadWorkspace();
            }
            catch (Exception ex)
            {
                ShowError(ex, "ManageQuizzes.btnSave");
            }
        }

        protected void btnDeleteQuiz_Click(object sender, EventArgs e)
        {
            AuthBLL.RequireAdmin();

            try
            {
                int quizId;
                if (int.TryParse(hidQuizId.Value, out quizId) && quizId > 0)
                    _quizBLL.DeleteQuiz(quizId);
                LoadWorkspace();
            }
            catch (Exception ex)
            {
                ShowError(ex, "ManageQuizzes.btnDelete");
            }
        }

        // ValidationException text is authored for the admin and safe to show.
        // Anything else gets logged instead, so a SQL or connection-string
        // fragment can never reach the rendered page.
        private void ShowError(Exception ex, string context)
        {
            var validation = ex as ValidationException;
            if (validation != null)
            {
                lblMessage.Text = HttpUtility.HtmlEncode(validation.Message);
            }
            else
            {
                ErrorLogger.Log(ex, context);
                lblMessage.Text = "The quiz could not be saved. Please try again.";
            }
            lblMessage.Visible = true;
        }
    }
}
