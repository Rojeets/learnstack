using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Member
{
    public partial class LessonViewer : Page
    {
        private readonly LessonBLL _lessonBLL = new LessonBLL();
        private readonly ProgressBLL _progressBLL = new ProgressBLL();

        private int _lessonId;

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthBLL.RequireLogin();

            if (!int.TryParse(Request.QueryString["LessonID"], out _lessonId))
            {
                ShowNotFound();
                return;
            }

            if (!_lessonBLL.IsLessonInPublishedCourse(_lessonId))
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
            Lesson lesson = _lessonBLL.GetLessonDetail(_lessonId);
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

            lnkBack.HRef = "CourseDetails.aspx?CourseID=" + _lessonBLL.GetCourseIdForLesson(_lessonId);
        }

        protected void btnMarkComplete_Click(object sender, EventArgs e)
        {
            AuthBLL.RequireLogin();

            // The user id comes from the session, never from a posted field, so
            // a student can only ever mark their own lessons complete.
            _progressBLL.MarkLessonComplete(AuthBLL.CurrentUserId, _lessonId);
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