using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Member
{
    public partial class QuizPage : Page
    {
        private readonly QuizBLL _quizBLL = new QuizBLL();

        private int _moduleId;
        private QuizForStudent _quiz;

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

            litQuizTitle.Text = HttpUtility.HtmlEncode(_quiz.QuizTitle);
            rptQuestions.DataSource = _quiz.Questions;
            rptQuestions.DataBind();
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

            int userId = AuthBLL.CurrentUserId;

            if (_quizBLL.HasPassedQuiz(userId, _quiz.QuizID))
            {
                pnlQuiz.Visible = false;
                pnlRetake.Visible = true;
                return;
            }

            try
            {
                Result result = _quizBLL.SubmitQuizAttempt(userId, _quiz.QuizID, selections);
                ShowResult(result);
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

        private void ShowResult(Result result)
        {
            pnlQuiz.Visible = false;
            pnlResult.Visible = true;

            litOutcome.Text = result.IsPassed ? "Passed" : "Not passed";
            litOutcome.CssClass = result.IsPassed ? "text-success" : "text-danger";
            litScore.Text = result.Score.ToString(System.Globalization.CultureInfo.InvariantCulture) + "%";
            litPassMark.Text = _quiz.PassMarkPercent.ToString() + "%";

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