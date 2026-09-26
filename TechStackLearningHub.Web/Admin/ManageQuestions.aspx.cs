using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;
using TechStackLearningHub.Web.Masterpages;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class ManageQuestions : Page
    {
        private readonly QuizBLL _quizBLL = new QuizBLL();
        private readonly QuestionBLL _questionBLL = new QuestionBLL();

        protected void Page_Load(object sender, EventArgs e)
        {
            // The admin shell (topbar title, highlighted sidebar link) lives in
            // Admin.Master and reads its state from these two calls, so they run
            // before the IsPostBack early-return: a postback render comes back
            // through here too, and would otherwise come up untitled.
            var master = (AdminMaster)Master;
            master.SetPageTitle("Manage Questions");
            master.SetActiveNav("ManageQuestions.aspx");

            if (!IsPostBack)
            {
                BindQuizSelector();
                int quizId;
                if (int.TryParse(Request.QueryString["QuizID"], out quizId))
                {
                    ListItem item = ddlQuiz.Items.FindByValue(quizId.ToString());
                    if (item != null)
                        ddlQuiz.SelectedValue = quizId.ToString();
                }
                BindBlankAnswerRows();
                LoadWorkspace();
            }
        }

        private void BindQuizSelector()
        {
            ddlQuiz.Items.Clear();
            ddlQuiz.Items.Add(new ListItem("-- choose a quiz --", ""));
            foreach (QuizListItem quiz in _quizBLL.GetAllQuizzesForAdmin())
            {
                ddlQuiz.Items.Add(new ListItem(
                    string.Format("{0} ({1} > {2})", quiz.QuizTitle, quiz.CourseName, quiz.ModuleTitle),
                    quiz.QuizID.ToString()));
            }
        }

        protected void ddlQuiz_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetEditor();
            LoadWorkspace();
        }

        private int CurrentQuizId
        {
            get
            {
                int quizId;
                return int.TryParse(ddlQuiz.SelectedValue, out quizId) ? quizId : 0;
            }
        }

        private void BindBlankAnswerRows()
        {
            var blanks = new List<Answer>();
            for (int i = 0; i < 4; i++)
                blanks.Add(new Answer { AnswerText = "", IsCorrect = false });
            rptAnswerRows.DataSource = blanks;
            rptAnswerRows.DataBind();
        }

        private void LoadWorkspace()
        {
            if (CurrentQuizId <= 0)
            {
                pnlWorkspace.Visible = false;
                return;
            }

            pnlWorkspace.Visible = true;
            grdQuestions.DataSource = _questionBLL.GetQuestionsByQuizId(CurrentQuizId);
            grdQuestions.DataBind();
        }

        protected void grdQuestions_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            AuthBLL.RequireAdmin();

            int questionId;
            if (!int.TryParse(e.CommandArgument as string, out questionId))
                return;

            switch (e.CommandName)
            {
                case "EditQuestion":
                    BeginEdit(questionId);
                    break;
                case "DeleteQuestion":
                    _questionBLL.DeleteQuestion(questionId);
                    ResetEditor();
                    LoadWorkspace();
                    break;
            }
        }

        private void BeginEdit(int questionId)
        {
            hidQuestionId.Value = questionId.ToString();
            txtQuestionText.Text = _questionBLL.GetQuestionsByQuizId(CurrentQuizId)
                .Find(q => q.QuestionID == questionId).QuestionText;
            txtMarks.Text = _questionBLL.GetQuestionsByQuizId(CurrentQuizId)
                .Find(q => q.QuestionID == questionId).Marks.ToString();

            List<Answer> answers = _questionBLL.GetAnswerOptions(questionId);
            rptAnswerRows.DataSource = answers;
            rptAnswerRows.DataBind();

            lblEditorHeading.InnerText = "Edit question";
            btnCancelEdit.Visible = true;
            lblMessage.Visible = false;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            AuthBLL.RequireAdmin();

            try
            {
                var answers = new List<AnswerInput>();
                foreach (RepeaterItem item in rptAnswerRows.Items)
                {
                    var txtAnswer = (TextBox)item.FindControl("txtAnswer");
                    var chkCorrect = (CheckBox)item.FindControl("chkCorrect");
                    answers.Add(new AnswerInput
                    {
                        AnswerText = txtAnswer.Text.Trim(),
                        IsCorrect = chkCorrect.Checked
                    });
                }

                int questionId;
                if (int.TryParse(hidQuestionId.Value, out questionId) && questionId > 0)
                {
                    _questionBLL.UpdateQuestion(new Question
                    {
                        QuestionID = questionId,
                        QuizID = CurrentQuizId,
                        QuestionText = txtQuestionText.Text.Trim(),
                        Marks = int.Parse(txtMarks.Text.Trim())
                    }, answers);
                }
                else
                {
                    _questionBLL.AddQuestionToQuiz(CurrentQuizId, txtQuestionText.Text.Trim(),
                        int.Parse(txtMarks.Text.Trim()), answers);
                }

                ResetEditor();
                LoadWorkspace();
            }
            catch (ValidationException ex)
            {
                lblMessage.Text = HttpUtility.HtmlEncode(ex.Message);
                lblMessage.Visible = true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "ManageQuestions.btnSave");
                lblMessage.Text = "The question could not be saved. Please try again.";
                lblMessage.Visible = true;
            }
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ResetEditor();
        }

        private void ResetEditor()
        {
            hidQuestionId.Value = "";
            txtQuestionText.Text = "";
            txtMarks.Text = "1";
            lblEditorHeading.InnerText = "New question";
            btnCancelEdit.Visible = false;
            lblMessage.Visible = false;
            BindBlankAnswerRows();
        }
    }
}
