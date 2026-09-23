using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.Web.Student
{
    public partial class LessonViewer : Page
    {
        private readonly LessonService _lessonService = new LessonService();
        private readonly ProgressService _progressService = new ProgressService();

        private int _lessonId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!int.TryParse(Request.QueryString["LessonID"], out _lessonId))
            {
                ShowNotFound();
                return;
            }

            if (!_lessonService.IsLessonInPublishedCourse(_lessonId))
            {
                ShowNotFound();
                return;
            }

            if (IsPostBack)
                return;

            LoadLesson();
        }

        private void LoadLesson()
        {
            Lesson lesson = _lessonService.GetLessonDetail(_lessonId);
            if (lesson == null)
            {
                ShowNotFound();
                return;
            }

            litLessonTitle.Text = HttpUtility.HtmlEncode(lesson.LessonTitle);
            litContentHtml.Text = lesson.ContentHTML;

            if (!string.IsNullOrEmpty(lesson.VideoUrl))
            {
                pnlVideo.Visible = true;
                frmVideo.Attributes["src"] = HttpUtility.HtmlAttributeEncode(lesson.VideoUrl);
            }

            if (!string.IsNullOrEmpty(lesson.NotesFilePath))
            {
                hlDownloadNotes.NavigateUrl = HttpUtility.HtmlAttributeEncode("~/" + lesson.NotesFilePath.TrimStart('~', '/'));
                hlDownloadNotes.Visible = true;
            }

            lnkBack.HRef = "CourseDetails.aspx?CourseID=" + _lessonService.GetCourseIdForLesson(_lessonId);
        }

        protected void btnMarkComplete_Click(object sender, EventArgs e)
        {
            int userId = (int)Session["UserID"];
            _progressService.MarkLessonComplete(userId, _lessonId);
            btnMarkComplete.Text = "Completed";
            btnMarkComplete.Enabled = false;
            btnMarkComplete.CssClass = "btn btn-outline-success";
        }

        private void ShowNotFound()
        {
            pnlLesson.Visible = false;
            pnlNotFound.Visible = true;
        }
    }
}