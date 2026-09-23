using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.Web.Student
{
    public partial class CourseDetails : Page
    {
        private readonly CourseService _courseService = new CourseService();
        private readonly ModuleService _moduleService = new ModuleService();
        private readonly LessonService _lessonService = new LessonService();
        private readonly QuizService _quizService = new QuizService();
        private readonly ProgressService _progressService = new ProgressService();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (IsPostBack)
                return;

            int courseId;
            if (!int.TryParse(Request.QueryString["CourseID"], out courseId))
            {
                pnlCourse.Visible = false;
                pnlNotFound.Visible = true;
                return;
            }

            // Students only ever see published courses, even via a hand-typed URL.
            Course course = _courseService.GetPublishedCourse(courseId);
            if (course == null)
            {
                pnlCourse.Visible = false;
                pnlNotFound.Visible = true;
                return;
            }

            litCourseName.Text = HttpUtility.HtmlEncode(course.CourseName);
            litTechStack.Text = HttpUtility.HtmlEncode(course.TechStack);
            litDescription.Text = HttpUtility.HtmlEncode(course.Description);

            int userId = (int)Session["UserID"];
            decimal percent = _progressService.GetCourseProgressSummary(userId, courseId);
            barProgress.Style["width"] = percent.ToString(System.Globalization.CultureInfo.InvariantCulture) + "%";

            rptModules.DataSource = _moduleService.GetModulesForCourse(courseId);
            rptModules.ItemDataBound += rptModules_ItemDataBound;
            rptModules.DataBind();
        }

        private void rptModules_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            int moduleId = (int)DataBinder.Eval(e.Item.DataItem, "ModuleID");

            Repeater rptLessons = (Repeater)e.Item.FindControl("rptLessons");
            rptLessons.DataSource = _lessonService.GetLessonsForModule(moduleId);
            rptLessons.DataBind();

            Repeater rptQuizzes = (Repeater)e.Item.FindControl("rptQuizzes");
            var quizzes = new List<Quiz>();
            Quiz quiz = _quizService.GetQuizForModuleId(moduleId);
            if (quiz != null)
                quizzes.Add(quiz);
            rptQuizzes.DataSource = quizzes;
            rptQuizzes.DataBind();
        }

        protected void rptLessons_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Open")
            {
                Response.Redirect("LessonViewer.aspx?LessonID=" + e.CommandArgument);
            }
        }
    }
}