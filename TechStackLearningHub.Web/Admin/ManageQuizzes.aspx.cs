using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class ManageQuizzes : Page
    {
        private readonly CourseService _courseService = new CourseService();
        private readonly ModuleService _moduleService = new ModuleService();
        private readonly QuizService _quizService = new QuizService();

        protected void Page_Load(object sender, EventArgs e)
        {
            RoleGuard.RequireAdmin(this);

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
            foreach (DataRow row in _courseService.GetAllCoursesForAdmin().Rows)
            {
                ddlCourse.Items.Add(new ListItem(
                    string.Format("{0} ({1})", row["CourseName"], row["TechStack"]),
                    row["CourseID"].ToString()));
            }
        }

        private void BindModuleOptions()
        {
            ddlModule.Items.Clear();
            ddlModule.Items.Add(new ListItem("-- choose a module --", ""));
            int courseId;
            if (int.TryParse(ddlCourse.SelectedValue, out courseId) && courseId > 0)
            {
                foreach (DataRow row in _moduleService.GetModulesForCourse(courseId).Rows)
                {
                    ddlModule.Items.Add(new ListItem(row["ModuleTitle"].ToString(), row["ModuleID"].ToString()));
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
            Quiz quiz = _quizService.GetQuizForModuleId(CurrentModuleId);
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
            try
            {
                int passMark = int.Parse(txtPassMark.Text.Trim());
                int quizId;
                if (int.TryParse(hidQuizId.Value, out quizId) && quizId > 0)
                    _quizService.UpdateQuiz(quizId, txtQuizTitle.Text.Trim(), passMark);
                else
                    _quizService.CreateQuiz(CurrentModuleId, txtQuizTitle.Text.Trim(), passMark);
                LoadWorkspace();
            }
            catch (Exception ex)
            {
                lblMessage.Text = HttpUtility.HtmlEncode(ex.Message);
                lblMessage.Visible = true;
            }
        }

        protected void btnDeleteQuiz_Click(object sender, EventArgs e)
        {
            try
            {
                int quizId;
                if (int.TryParse(hidQuizId.Value, out quizId) && quizId > 0)
                    _quizService.DeleteQuiz(quizId);
                LoadWorkspace();
            }
            catch (Exception ex)
            {
                lblMessage.Text = HttpUtility.HtmlEncode(ex.Message);
                lblMessage.Visible = true;
            }
        }
    }
}