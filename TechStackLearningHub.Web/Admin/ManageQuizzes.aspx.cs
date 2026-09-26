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
    public partial class ManageQuizzes : Page
    {
        private readonly CourseBLL _courseBLL = new CourseBLL();
        private readonly ModuleBLL _moduleBLL = new ModuleBLL();
        private readonly QuizBLL _quizBLL = new QuizBLL();

        protected void Page_Load(object sender, EventArgs e)
        {
            // The admin shell (topbar title, highlighted sidebar link) lives in
            // Admin.Master and reads its state from these two calls, so they run
            // before the IsPostBack early-return: a postback render comes back
            // through here too, and would otherwise come up untitled.
            var master = (AdminMaster)Master;
            master.SetPageTitle("Manage Quizzes");
            master.SetActiveNav("ManageQuizzes.aspx");

            if (!IsPostBack)
            {
                BindCourseSelector();
                int courseId;
                bool haveCourse = int.TryParse(Request.QueryString["CourseID"], out courseId) && courseId > 0;
                if (haveCourse)
                {
                    ListItem item = ddlCourse.Items.FindByValue(courseId.ToString());
                    if (item != null)
                        ddlCourse.SelectedValue = courseId.ToString();
                }
                BindModuleOptions();
                int moduleId;
                if (int.TryParse(Request.QueryString["ModuleID"], out moduleId) && moduleId > 0)
                {
                    if (!SelectModule(moduleId) && !haveCourse)
                    {
                        // ModuleID-only link: the module dropdown came back empty
                        // because no course was selected, so resolve the parent
                        // course from the module and retry. Without this the page
                        // rendered an empty workspace and the deep link was dead.
                        int derivedCourseId = _moduleBLL.GetCourseIdForModule(moduleId);
                        if (derivedCourseId > 0)
                        {
                            ListItem derived = ddlCourse.Items.FindByValue(derivedCourseId.ToString());
                            if (derived != null)
                            {
                                ddlCourse.SelectedValue = derivedCourseId.ToString();
                                BindModuleOptions();
                                SelectModule(moduleId);
                            }
                        }
                    }
                }
                LoadWorkspace();
            }
        }

        /// <summary>Selects a module in the dropdown, reporting whether it was there.</summary>
        private bool SelectModule(int moduleId)
        {
            ListItem moduleItem = ddlModule.Items.FindByValue(moduleId.ToString());
            if (moduleItem == null)
                return false;
            ddlModule.SelectedValue = moduleId.ToString();
            return true;
        }

        private void BindCourseSelector()
        {
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new ListItem("-- choose a course --", ""));
            foreach (Course course in _courseBLL.GetAllCoursesForAdmin())
            {
                ddlCourse.Items.Add(new ListItem(
                    string.Format("{0} ({1})", course.CourseName, course.TechStack),
                    course.CourseID.ToString()));
            }
        }

        private void BindModuleOptions()
        {
            ddlModule.Items.Clear();
            ddlModule.Items.Add(new ListItem("-- choose a module --", ""));
            int courseId;
            if (int.TryParse(ddlCourse.SelectedValue, out courseId) && courseId > 0)
            {
                foreach (Module module in _moduleBLL.GetModulesForCourse(courseId))
                {
                    ddlModule.Items.Add(new ListItem(module.ModuleTitle, module.ModuleID.ToString()));
                }
            }
        }

        protected void ddlCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindModuleOptions();
            LoadWorkspace();
        }

        protected void ddlModule_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadWorkspace();
        }

        private int CurrentModuleId
        {
            get
            {
                int moduleId;
                return int.TryParse(ddlModule.SelectedValue, out moduleId) ? moduleId : 0;
            }
        }

        private void LoadWorkspace()
        {
            if (CurrentModuleId <= 0)
            {
                pnlWorkspace.Visible = false;
                return;
            }

            pnlWorkspace.Visible = true;
            Quiz quiz = _quizBLL.GetQuizForModuleId(CurrentModuleId);

            // A module owns at most one quiz - QuizBLL.CreateQuiz refuses to
            // create a second one - so the grid for the selected module is
            // either that single row or the EmptyDataText row. It is bound
            // from the very call the editor below already made rather than a
            // new BLL method, so the grid and the form can never end up
            // showing different quizzes for the same module.
            var rows = new List<Quiz>();
            if (quiz != null)
                rows.Add(quiz);
            grdQuizzes.DataSource = rows;
            grdQuizzes.DataBind();

            LoadEditor(quiz);
        }

        // Puts the module's quiz (or the blank "create" state) into the form.
        // Split out of LoadWorkspace so the grid's Edit command can fill the
        // same fields without duplicating the assignment list.
        private void LoadEditor(Quiz quiz)
        {
            lnkManageQuestions.NavigateUrl = quiz == null
                ? "#"
                : HttpUtility.HtmlAttributeEncode("ManageQuestions.aspx?QuizID=" + quiz.QuizID);
            lnkManageQuestions.Enabled = quiz != null;

            btnDeleteQuiz.OnClientClick = AdminUi.Confirm(
                "Delete {0} and all its questions? This cannot be undone.",
                quiz == null ? "this quiz" : quiz.QuizTitle);

            if (quiz == null)
            {
                hidQuizId.Value = "";
                txtQuizTitle.Text = "";
                txtPassMark.Text = "50";
                txtDurationMinutes.Text = QuizBLL.DefaultDurationMinutes.ToString();
                litHeading.Text = "Create quiz for this module";
            }
            else
            {
                hidQuizId.Value = quiz.QuizID.ToString();
                txtQuizTitle.Text = quiz.QuizTitle;
                txtPassMark.Text = quiz.PassMarkPercent.ToString();
                // Show the effective limit, not the raw NULL, so the admin edits
                // the number the student will actually be given.
                txtDurationMinutes.Text =
                    QuizBLL.ResolveDurationMinutes(quiz.DurationMinutes).ToString();
                litHeading.Text = "Module quiz";
            }
            btnCancelEdit.Visible = false;
            lblMessage.Visible = false;
            lblSuccess.Visible = false;
        }

        protected void grdQuizzes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Postback handlers run before Page_Load, so each one that writes
            // has to re-assert admin rights itself. Admin.Master.Page_Init stops
            // the request before the handler is ever reached, but defence in
            // depth here means no future refactor of the master can silently
            // turn this page into an open write endpoint.
            AuthBLL.RequireAdmin();

            int quizId;
            if (!int.TryParse(e.CommandArgument as string, out quizId))
                return;

            try
            {
                switch (e.CommandName)
                {
                    case "EditQuiz":
                        BeginEdit(quizId);
                        // No rebind here: the row data did not change, and
                        // LoadWorkspace would immediately undo the edit state
                        // this command just set up.
                        return;
                    case "DeleteQuiz":
                        string removedTitle = _quizBLL.GetQuizForModuleId(CurrentModuleId) == null
                            ? "the quiz"
                            : _quizBLL.GetQuizForModuleId(CurrentModuleId).QuizTitle;
                        DeleteQuizById(quizId);
                        LoadWorkspace();
                        ShowSuccess("Deleted \"" + removedTitle + "\".");
                        return;
                    default:
                        // Not one of our commands - GridView raises other
                        // command names of its own. Nothing to do.
                        return;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex, "ManageQuizzes.grdQuizzes_RowCommand");
                LoadWorkspace();
            }
        }

        private void BeginEdit(int quizId)
        {
            // QuizBLL exposes no GetQuizById, and a module can only own one
            // quiz, so the row the admin clicked is the selected module's own
            // quiz: re-read it through the same per-module call the grid and
            // the editor use, and only proceed if the id still matches.
            Quiz quiz = _quizBLL.GetQuizForModuleId(CurrentModuleId);
            if (quiz == null || quiz.QuizID != quizId)
                return;

            LoadEditor(quiz);
            litHeading.Text = "Edit quiz";
            btnCancelEdit.Visible = true;
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            // Re-read the module rather than clearing the fields: for a module
            // that already has a quiz the correct "cancelled" state is that
            // quiz, not an empty create form.
            LoadWorkspace();
        }

        protected void btnSaveQuiz_Click(object sender, EventArgs e)
        {
            AuthBLL.RequireAdmin();

            try
            {
                int passMark = int.Parse(txtPassMark.Text.Trim());
                int durationMinutes = int.Parse(txtDurationMinutes.Text.Trim());
                bool wasEdit = !string.IsNullOrEmpty(hidQuizId.Value);
                string title = txtQuizTitle.Text.Trim();
                int quizId;
                if (int.TryParse(hidQuizId.Value, out quizId) && quizId > 0)
                    _quizBLL.UpdateQuiz(quizId, title, passMark, durationMinutes);
                else
                    _quizBLL.CreateQuiz(CurrentModuleId, title, passMark, durationMinutes);
                LoadWorkspace();
                ShowSuccess(wasEdit ? "Saved \"" + title + "\"." : "Created \"" + title + "\".");
            }
            catch (Exception ex)
            {
                ShowError(ex, "ManageQuizzes.btnSave");
            }
        }

        protected void btnDeleteQuiz_Click(object sender, EventArgs e)
        {
            AuthBLL.RequireAdmin();

            try
            {
                int quizId;
                if (int.TryParse(hidQuizId.Value, out quizId) && quizId > 0)
                {
                    Quiz existing = _quizBLL.GetQuizForModuleId(CurrentModuleId);
                    string title = existing == null ? "the quiz" : existing.QuizTitle;
                    DeleteQuizById(quizId);
                    LoadWorkspace();
                    ShowSuccess("Deleted \"" + title + "\".");
                }
                else
                {
                    LoadWorkspace();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex, "ManageQuizzes.btnDelete");
            }
        }

        /// <summary>
        /// The single delete path for this page. The editor's Delete button and
        /// the grid's DeleteQuiz command both come through here, so the BLL
        /// rules (e.g. a quiz with attempts cannot be deleted) can never be
        /// enforced in one place and forgotten in the other. Reloading the
        /// workspace stays with the callers, which know whether they are in an
        /// event handler or a row command.
        /// </summary>
        private void DeleteQuizById(int quizId)
        {
            _quizBLL.DeleteQuiz(quizId);
        }

        /// <summary>
        /// Renders a quiz's stored time limit for the grid. Exposed as a public
        /// method so the markup can call it without an inline expression that
        /// would have to resolve the BLL constant itself.
        /// </summary>
        public string FormatDuration(object storedMinutes)
        {
            int? minutes;
            try
            {
                minutes = storedMinutes == null || storedMinutes == DBNull.Value
                    ? (int?)null
                    : Convert.ToInt32(storedMinutes);
            }
            catch (Exception)
            {
                minutes = null;
            }

            if (!minutes.HasValue)
                return QuizBLL.DefaultDurationMinutes + " min (default)";
            return minutes.Value + " min";
        }

        // ValidationException text is authored for the admin and safe to show.
        // Anything else gets logged instead, so a SQL or connection-string
        // fragment can never reach the rendered page.
        private void ShowError(Exception ex, string context)
        {
            AdminUi.Error(lblMessage, lblSuccess, ex, "The quiz could not be saved. Please try again.", context);
        }

        private void ShowSuccess(string message)
        {
            AdminUi.Success(lblMessage, lblSuccess, message);
        }
    }
}
