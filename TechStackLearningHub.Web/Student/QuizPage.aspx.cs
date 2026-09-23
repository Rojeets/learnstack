using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.Web.Student
{
    public partial class QuizPage : Page
    {
        private readonly QuizService _quizService = new QuizService();

        private int _moduleId;
        private QuizForStudent _quiz;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!int.TryParse(Request.QueryString["ModuleID"], out _moduleId))
            {
                ShowNotFound();
                return;
            }

            _quiz = _quizService.GetQuizForStudentIfPublished(_moduleId);
            if (_quiz == null)
            {
                ShowNotFound();
                return;
            }

            if (IsPostBack)
                return;

            LoadQuiz();
        }

        private void LoadQuiz()
        {
            int userId = (int)Session["UserID"];

            if (_quizService.HasPassedQuiz(userId, _quiz.QuizID))
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

            int userId = (int)Session["UserID"];
            Result result = _quizService.SubmitQuizAttempt(userId, _quiz.QuizID, selections);

            ShowResult(result);
        }

        private void ShowResult(Result result)
        {
            pnlQuiz.Visible = false;
            pnlResult.Visible = true;

            litOutcome.Text = result.IsPassed ? "Passed" : "Not passed";
            litOutcome.CssClass = result.IsPassed ? "text-success" : "text-danger";
            litScore.Text = result.Score.ToString(System.Globalization.CultureInfo.InvariantCulture) + "%";
            litPassMark.Text = _quiz.PassMarkPercent.ToString() + "%";

            int courseId = _quizService.GetModuleCourseId(_moduleId);
            lnkBackToCourse.HRef = HttpUtility.HtmlAttributeEncode("CourseDetails.aspx?CourseID=" + courseId);
        }

        private void ShowNotFound()
        {
            pnlQuiz.Visible = false;
            pnlNotFound.Visible = true;
        }
    }
}