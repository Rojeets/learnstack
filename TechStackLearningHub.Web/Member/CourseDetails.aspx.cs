using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Member
{
    public partial class CourseDetails : Page
    {
        private readonly CourseBLL _courseBLL = new CourseBLL();
        private readonly ModuleBLL _moduleBLL = new ModuleBLL();
        private readonly LessonBLL _lessonBLL = new LessonBLL();
        private readonly QuizBLL _quizBLL = new QuizBLL();
        private readonly ProgressBLL _progressBLL = new ProgressBLL();

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthBLL.RequireLogin();

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
            Course course = _courseBLL.GetPublishedCourse(courseId);
            if (course == null)
            {
                pnlCourse.Visible = false;
                pnlNotFound.Visible = true;
                return;
            }

            litCourseName.Text = HttpUtility.HtmlEncode(course.CourseName);
            litTechStack.Text = HttpUtility.HtmlEncode(course.TechStack);
            litDescription.Text = HttpUtility.HtmlEncode(course.Description);

            int userId = AuthBLL.CurrentUserId;
            decimal percent = _progressBLL.GetCourseProgressSummary(userId, courseId);
            string percentText = percent.ToString(System.Globalization.CultureInfo.InvariantCulture);
            barProgress.Style["width"] = percentText + "%";

            // The accessible value has to track the same number the bar is drawn
            // from, so both are derived from the one `percent` above.
            pnlProgress.Attributes["aria-valuenow"] = percentText;
            pnlProgress.Attributes["aria-valuetext"] = percentText + "% complete";
            lblProgressSummary.Text = percent <= 0
                ? "You have not started this course yet."
                : percent >= 100
                    ? "All lessons in this course are complete."
                    : "You have completed " + percentText + "% of this course.";

            rptModules.DataSource = _moduleBLL.GetModulesForCourse(courseId);
            rptModules.ItemDataBound += rptModules_ItemDataBound;
            rptModules.DataBind();
        }

        private void rptModules_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            int moduleId = (int)DataBinder.Eval(e.Item.DataItem, "ModuleID");

            Repeater rptLessons = (Repeater)e.Item.FindControl("rptLessons");
            rptLessons.DataSource = _lessonBLL.GetLessonsForModule(moduleId);
            rptLessons.DataBind();

            Repeater rptQuizzes = (Repeater)e.Item.FindControl("rptQuizzes");
            var quizzes = new List<Quiz>();
            Quiz quiz = _quizBLL.GetQuizForModuleId(moduleId);
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