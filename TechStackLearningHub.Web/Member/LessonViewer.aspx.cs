using System;
using System.Collections.Generic;
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

            RenderCompletionState(AuthBLL.CurrentUserId);
            RenderLessonNavigation(lesson);
        }

        /// <summary>
        /// The completion state comes from the database on every render. The
        /// button used to be the only source of truth and only ever reflected the
        /// browser's own optimistic text, so reloading a finished lesson offered
        /// "Mark as Complete" all over again.
        /// </summary>
        private void RenderCompletionState(int userId)
        {
            bool complete = _progressBLL.IsLessonComplete(userId, _lessonId);

            pnlCompleted.CssClass = complete
                ? "alert alert-success"
                : "alert alert-success d-none";

            if (!complete)
            {
                btnMarkComplete.Visible = true;
                btnMarkComplete.Enabled = true;
                btnMarkComplete.Text = "Mark as complete";
                btnMarkComplete.CssClass = "btn btn-success";
                return;
            }

            btnMarkComplete.Visible = false;
        }

        /// <summary>
        /// Previous/next lesson within the same module, ordered by LessonOrder.
        /// Completing a lesson used to be a dead end: the only way onward was back
        /// through the course page, with no indication of what came next.
        /// </summary>
        private void RenderLessonNavigation(Lesson lesson)
        {
            List<Lesson> siblings = _lessonBLL.GetLessonsForModule(lesson.ModuleID);
            int index = siblings.FindIndex(l => l.LessonID == lesson.LessonID);
            if (index < 0)
            {
                lnkPrevLesson.Visible = false;
                lnkNextLesson.Visible = false;
                return;
            }

            if (index > 0)
            {
                lnkPrevLesson.NavigateUrl = HttpUtility.HtmlAttributeEncode(
                    "LessonViewer.aspx?LessonID=" + siblings[index - 1].LessonID);
                lnkPrevLesson.Text = "&laquo; Previous: " + HttpUtility.HtmlEncode(siblings[index - 1].LessonTitle);
                lnkPrevLesson.Visible = true;
            }
            else
            {
                lnkPrevLesson.Visible = false;
            }

            if (index < siblings.Count - 1)
            {
                lnkNextLesson.NavigateUrl = HttpUtility.HtmlAttributeEncode(
                    "LessonViewer.aspx?LessonID=" + siblings[index + 1].LessonID);
                lnkNextLesson.Text = "Next: " + HttpUtility.HtmlEncode(siblings[index + 1].LessonTitle) + " &raquo;";
                lnkNextLesson.Visible = true;
            }
            else
            {
                lnkNextLesson.Visible = false;
            }
        }

        protected void btnMarkComplete_Click(object sender, EventArgs e)
        {
            AuthBLL.RequireLogin();

            // The user id comes from the session, never from a posted field, so
            // a student can only ever mark their own lessons complete.
            int userId = AuthBLL.CurrentUserId;
            _progressBLL.MarkLessonComplete(userId, _lessonId);

            // Re-read the state instead of trusting what the click implied, so
            // the page and the database can never disagree.
            RenderCompletionState(userId);
        }

        private void ShowNotFound()
        {
            pnlLesson.Visible = false;
            pnlNotFound.Visible = true;
        }
    }
}