using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Member
{
    public partial class QuizPage : Page
    {
        /// <summary>
        /// Session key prefix for an in-flight attempt's start instant. Keyed by
        /// user and quiz so one student's clock can never be applied to another's
        /// attempt, and a failed quiz's retake gets its own fresh clock.
        /// </summary>
        private const string AttemptStartKeyPrefix = "QuizAttemptStartUtc:";

        private readonly QuizBLL _quizBLL = new QuizBLL();

        private int _moduleId;
        private QuizForStudent _quiz;
        private bool _timeExpired;

        /// <summary>
        /// Absolute UTC instant, in milliseconds since the Unix epoch, at which
        /// this attempt expires. Emitted into the page so the countdown is
        /// derived from server state rather than a hardcoded local counter.
        /// </summary>
        public long QuizDeadlineUtcMs { get; private set; }

        /// <summary>Whole seconds left at render time, floored at zero.</summary>
        public int QuizDurationSeconds { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthBLL.RequireLogin();

            if (!int.TryParse(Request.QueryString["ModuleID"], out _moduleId))
            {
                ShowNotFound();
                return;
            }

            _quiz = _quizBLL.GetQuizForStudentIfPublished(_moduleId);
            if (_quiz == null)
            {
                ShowNotFound();
                return;
            }

            LoadQuiz();
        }

        private void LoadQuiz()
        {
            int userId = AuthBLL.CurrentUserId;

            if (_quizBLL.HasPassedQuiz(userId, _quiz.QuizID))
            {
                pnlQuiz.Visible = false;
                pnlRetake.Visible = true;
                return;
            }

            DateTime deadline = GetOrStartAttempt(userId, _quiz.QuizID)
                .AddMinutes(_quiz.DurationMinutes);
            TimeSpan remaining = deadline - DateTime.UtcNow;

            _timeExpired = remaining <= TimeSpan.Zero;
            QuizDurationSeconds = (int)Math.Max(0, Math.Ceiling(remaining.TotalSeconds));
            QuizDeadlineUtcMs = ToUnixMilliseconds(deadline);

            litQuizTitle.Text = HttpUtility.HtmlEncode(_quiz.QuizTitle);
            rptQuestions.DataSource = _quiz.Questions;
            rptQuestions.DataBind();
        }

        /// <summary>
        /// Reads the attempt's start instant out of the session, stamping one on
        /// first view. A start in the future means the clock moved or the item was
        /// tampered with, so it is reset to now rather than being trusted.
        /// </summary>
        private DateTime GetOrStartAttempt(int userId, int quizId)
        {
            string key = AttemptStartKeyPrefix + userId + ":" + quizId;
            DateTime now = DateTime.UtcNow;

            var stored = Session[key] as DateTime?;
            if (stored.HasValue && stored.Value <= now)
                return stored.Value;

            Session[key] = now;
            return now;
        }

        private void ClearAttempt(int userId, int quizId)
        {
            Session.Remove(AttemptStartKeyPrefix + userId + ":" + quizId);
        }

        private static long ToUnixMilliseconds(DateTime utc)
        {
            return (long)(utc - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        protected void rptQuestions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            StudentQuestion question = (StudentQuestion)e.Item.DataItem;
            PlaceHolder phAnswers = (PlaceHolder)e.Item.FindControl("phAnswers");
            if (phAnswers == null)
                return;

            // Determine the number of options surfaced to the student based on
            // the answer type; IsCorrect is never present on the DTO.
            if (question.AllowMultiple)
            {
                var cbl = new CheckBoxList();
                cbl.ID = "cbxList";
                foreach (StudentAnswer answer in question.Answers)
                    cbl.Items.Add(new ListItem(answer.AnswerText, answer.AnswerID.ToString()));
                phAnswers.Controls.Add(cbl);
            }
            else
            {
                var rbl = new RadioButtonList();
                rbl.ID = "radList";
                foreach (StudentAnswer answer in question.Answers)
                    rbl.Items.Add(new ListItem(answer.AnswerText, answer.AnswerID.ToString()));
                phAnswers.Controls.Add(rbl);
            }
        }

        protected void btnSubmitQuiz_Click(object sender, EventArgs e)
        {
            AuthBLL.RequireLogin();

            if (_quiz == null)
                return;

            int userId = AuthBLL.CurrentUserId;

            // Read the clock and the retake state before touching the answer
            // controls: this also re-asserts that a second, concurrent submit of
            // the same attempt cannot slip past the guard below.
            bool timeExpired = _timeExpired;
            if (_quizBLL.HasPassedQuiz(userId, _quiz.QuizID))
            {
                pnlQuiz.Visible = false;
                pnlRetake.Visible = true;
                return;
            }

            // Because the timer auto-submits, a quiz can be judged and recorded
            // exactly once; the service guards against double submission.
            var selections = new Dictionary<int, List<int>>();

            foreach (RepeaterItem item in rptQuestions.Items)
            {
                if (item.ItemType != ListItemType.Item && item.ItemType != ListItemType.AlternatingItem)
                    continue;

                Literal litId = (Literal)item.FindControl("litQuestionId");
                PlaceHolder phAnswers = (PlaceHolder)item.FindControl("phAnswers");
                if (litId == null || phAnswers == null)
                    continue;

                int questionId;
                if (!int.TryParse(litId.Text, out questionId))
                    continue;

                var selectedIds = new List<int>();
                RadioButtonList rbl = (RadioButtonList)phAnswers.FindControl("radList");
                CheckBoxList cbl = (CheckBoxList)phAnswers.FindControl("cbxList");

                if (rbl != null)
                {
                    foreach (ListItem li in rbl.Items)
                    {
                        if (li.Selected)
                            selectedIds.Add(int.Parse(li.Value));
                    }
                }
                else if (cbl != null)
                {
                    foreach (ListItem li in cbl.Items)
                    {
                        if (li.Selected)
                            selectedIds.Add(int.Parse(li.Value));
                    }
                }

                selections[questionId] = selectedIds;
            }

            try
            {
                Result result = _quizBLL.SubmitQuizAttempt(userId, _quiz.QuizID, selections);
                // The attempt is now recorded, so the clock has done its job. A
                // retake of a failed quiz starts a fresh one.
                ClearAttempt(userId, _quiz.QuizID);
                ShowResult(result, timeExpired);
            }
            catch (ValidationException ex)
            {
                // The retake guard raises this; the text is authored for the
                // student, so it can be shown as-is.
                litOutcome.Text = HttpUtility.HtmlEncode(ex.Message);
                litOutcome.CssClass = "text-danger";
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "QuizPage.btnSubmitQuiz");
                litOutcome.Text = "Your attempt could not be recorded. Please try again.";
                litOutcome.CssClass = "text-danger";
            }
        }

        private void ShowResult(Result result, bool timeExpired)
        {
            pnlQuiz.Visible = false;
            pnlResult.Visible = true;

            litOutcome.Text = result.IsPassed ? "Passed" : "Not passed";
            litOutcome.CssClass = result.IsPassed ? "text-success" : "text-danger";
            litScore.Text = result.Score.ToString(System.Globalization.CultureInfo.InvariantCulture) + "%";
            litPassMark.Text = _quiz.PassMarkPercent.ToString() + "%";

            // Explain the grade when it was not the student's own click that
            // closed the attempt, so an auto-submitted result is never mistaken
            // for a lost submission.
            if (timeExpired)
            {
                HtmlGenericControl notice = (HtmlGenericControl)pnlResult.FindControl("quizExpiredNotice");
                if (notice != null)
                    notice.Attributes["class"] = "alert alert-warning";
            }

            int courseId = _quizBLL.GetModuleCourseId(_moduleId);
            lnkBackToCourse.HRef = HttpUtility.HtmlAttributeEncode("CourseDetails.aspx?CourseID=" + courseId);
        }

        private void ShowNotFound()
        {
            pnlQuiz.Visible = false;
            pnlNotFound.Visible = true;
        }
    }
}