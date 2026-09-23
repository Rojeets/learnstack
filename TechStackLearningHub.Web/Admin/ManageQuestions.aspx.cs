using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class ManageQuestions : Page
    {
        private readonly QuizService _quizService = new QuizService();
        private readonly QuestionService _questionService = new QuestionService();

        protected void Page_Load(object sender, EventArgs e)
        {
            RoleGuard.RequireAdmin(this);

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
            foreach (DataRow row in _quizService.GetAllQuizzesForAdmin().Rows)
            {
                ddlQuiz.Items.Add(new ListItem(
                    string.Format("{0} ({1} > {2})", row["QuizTitle"], row["CourseName"], row["ModuleTitle"]),
                    row["QuizID"].ToString()));
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
            grdQuestions.DataSource = _questionService.GetQuestionsByQuizId(CurrentQuizId);
            grdQuestions.DataBind();
        }

        protected void grdQuestions_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int questionId;
            if (!int.TryParse(e.CommandArgument as string, out questionId))
                return;

            switch (e.CommandName)
            {
                case "EditQuestion":
                    BeginEdit(questionId);
                    break;
                case "DeleteQuestion":
                    _questionService.DeleteQuestion(questionId);
                    ResetEditor();
                    LoadWorkspace();
                    break;
            }
        }

        private void BeginEdit(int questionId)
        {
            hidQuestionId.Value = questionId.ToString();
            txtQuestionText.Text = _questionService.GetQuestionsByQuizId(CurrentQuizId)
                .Find(q => q.QuestionID == questionId).QuestionText;
            txtMarks.Text = _questionService.GetQuestionsByQuizId(CurrentQuizId)
                .Find(q => q.QuestionID == questionId).Marks.ToString();

            List<Answer> answers = _questionService.GetAnswerOptions(questionId);
            rptAnswerRows.DataSource = answers;
            rptAnswerRows.DataBind();

            lblEditorHeading.InnerText = "Edit question";
            btnCancelEdit.Visible = true;
            lblMessage.Visible = false;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
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
                    _questionService.UpdateQuestion(new Question
                    {
                        QuestionID = questionId,
                        QuizID = CurrentQuizId,
                        QuestionText = txtQuestionText.Text.Trim(),
                        Marks = int.Parse(txtMarks.Text.Trim())
                    }, answers);
                }
                else
                {
                    _questionService.AddQuestionToQuiz(CurrentQuizId, txtQuestionText.Text.Trim(),
                        int.Parse(txtMarks.Text.Trim()), answers);
                }

                ResetEditor();
                LoadWorkspace();
            }
            catch (Exception ex)
            {
                lblMessage.Text = HttpUtility.HtmlEncode(ex.Message);
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