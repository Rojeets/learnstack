using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;
using TechStackLearningHub.Web.Masterpages;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class ManageLessons : Page
    {
        private readonly CourseBLL _courseBLL = new CourseBLL();
        private readonly ModuleBLL _moduleBLL = new ModuleBLL();
        private readonly LessonBLL _lessonBLL = new LessonBLL();

        private static readonly string[] AllowedNoteExtensions = { ".pdf", ".docx", ".doc", ".txt" };
        private const long MaxNoteBytes = 5 * 1024 * 1024;

        protected void Page_Load(object sender, EventArgs e)
        {
            // The admin shell (topbar title, highlighted sidebar link) lives in
            // Admin.Master and reads its state from these two calls, so they run
            // before the IsPostBack early-return: a postback render comes back
            // through here too, and would otherwise come up untitled.
            var master = (AdminMaster)Master;
            master.SetPageTitle("Manage Lessons");
            master.SetActiveNav("ManageLessons.aspx");

            if (!IsPostBack)
            {
                BindCourseAndModuleSelectors();
                LoadWorkspace();
            }
        }

        private void BindCourseAndModuleSelectors()
        {
            ddlCourse.Items.Clear();
            ddlCourse.Items.Add(new ListItem("-- choose a course --", ""));
            foreach (Course course in _courseBLL.GetAllCoursesForAdmin())
            {
                ddlCourse.Items.Add(new ListItem(
                    string.Format("{0} ({1})", course.CourseName, course.TechStack),
                    course.CourseID.ToString()));
            }

            int courseId;
            bool haveCourse = int.TryParse(Request.QueryString["CourseID"], out courseId) && courseId > 0;
            if (haveCourse)
            {
                ListItem item = ddlCourse.Items.FindByValue(courseId.ToString());
                if (item != null)
                    ddlCourse.SelectedValue = courseId.ToString();
            }

            int moduleId;
            bool haveModule = int.TryParse(Request.QueryString["ModuleID"], out moduleId) && moduleId > 0;

            // A ModuleID-only link is what the ManageModules "Lessons" action
            // produces. The module dropdown is populated from the selected
            // course, so without resolving the parent course here the module
            // could never be selected and the page came up empty. Deriving the
            // course makes the deep link work on its own, whether or not the
            // caller also sent CourseID.
            if (!haveCourse && haveModule)
            {
                int derivedCourseId = _moduleBLL.GetCourseIdForModule(moduleId);
                if (derivedCourseId > 0)
                {
                    ListItem derived = ddlCourse.Items.FindByValue(derivedCourseId.ToString());
                    if (derived != null)
                        ddlCourse.SelectedValue = derivedCourseId.ToString();
                }
            }

            BindModuleOptions();

            if (haveModule)
            {
                ListItem moduleItem = ddlModule.Items.FindByValue(moduleId.ToString());
                if (moduleItem != null)
                    ddlModule.SelectedValue = moduleId.ToString();
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
            ResetEditor();
            LoadWorkspace();
        }

        protected void ddlModule_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetEditor();
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

        private int CurrentCourseId
        {
            get
            {
                int courseId;
                return int.TryParse(ddlCourse.SelectedValue, out courseId) ? courseId : 0;
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

            // A module owns at most one quiz, so this is a module-level action
            // rather than a per-lesson one. Without it there was no way to reach
            // a module's quiz from its lessons at all.
            lnkManageQuizzes.Visible = true;
            lnkManageQuizzes.NavigateUrl = HttpUtility.HtmlAttributeEncode(
                "ManageQuizzes.aspx?CourseID=" + CurrentCourseId + "&ModuleID=" + CurrentModuleId);

            grdLessons.DataSource = _lessonBLL.GetLessonsForModule(CurrentModuleId);
            grdLessons.DataBind();
        }

        protected void grdLessons_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Postback handlers run before Page_Load, so each one that writes
            // has to re-assert admin rights itself. Admin.Master.Page_Init stops
            // the request before the handler is ever reached, but defence in
            // depth here means no future refactor of the master can silently
            // turn this page into an open write endpoint.
            AuthBLL.RequireAdmin();

            int lessonId;
            if (!int.TryParse(e.CommandArgument as string, out lessonId))
                return;

            switch (e.CommandName)
            {
                case "EditLesson":
                    BeginEdit(lessonId);
                    break;
                case "DeleteLesson":
                    Lesson removed = _lessonBLL.GetLessonDetail(lessonId);
                    _lessonBLL.DeleteLesson(lessonId);
                    ResetEditor();
                    LoadWorkspace();
                    ShowSuccess("Deleted \"" + (removed == null ? "the lesson" : removed.LessonTitle) + "\".");
                    break;
            }
        }

        private void BeginEdit(int lessonId)
        {
            Lesson lesson = _lessonBLL.GetLessonDetail(lessonId);
            if (lesson == null)
                return;

            hidLessonId.Value = lessonId.ToString();
            txtLessonTitle.Text = lesson.LessonTitle;
            txtContentHtml.Text = lesson.ContentHTML ?? "";
            txtVideoUrl.Text = lesson.VideoUrl ?? "";
            lblCurrentNotes.Text = string.IsNullOrEmpty(lesson.NotesFilePath)
                ? "No notes file uploaded."
                : "Current: " + Path.GetFileName(lesson.NotesFilePath);
            lblEditorHeading.InnerText = "Edit lesson";
            btnCancelEdit.Visible = true;
            lblMessage.Visible = false;
            lblSuccess.Visible = false;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            AuthBLL.RequireAdmin();

            try
            {
                bool wasEdit = !string.IsNullOrEmpty(hidLessonId.Value);
                string notesPath = SaveUploadedNotes();
                string title = txtLessonTitle.Text.Trim();
                ApplyLesson(notesPath);
                ResetEditor();
                LoadWorkspace();
                ShowSuccess(wasEdit ? "Saved \"" + title + "\"." : "Added \"" + title + "\".");
            }
            catch (Exception ex)
            {
                // A ValidationException is authored, user-facing text. Anything
                // else is a fault, not feedback: a dead connection or a
                // constraint violation must not leak its message (and with it
                // the connection string) into the page source.
                ShowError(ex, "ManageLessons.btnSave");
            }
        }

        private string SaveUploadedNotes()
        {
            if (!fupNotes.HasFile)
                return null;

            string extension = Path.GetExtension(fupNotes.FileName).ToLowerInvariant();
            if (Array.IndexOf(AllowedNoteExtensions, extension) < 0)
                throw new ValidationException("Notes file must be a PDF, DOCX, DOC or TXT file.");
            if (fupNotes.PostedFile.ContentLength > MaxNoteBytes)
                throw new ValidationException("Notes file must be 5 MB or smaller.");

            string folder = Server.MapPath("~/UploadedNotes");
            Directory.CreateDirectory(folder);
            string storedName = Guid.NewGuid().ToString("N") + extension;
            fupNotes.SaveAs(Path.Combine(folder, storedName));
            return "UploadedNotes/" + storedName;
        }

        private void ApplyLesson(string newNotesPath)
        {
            int lessonId;
            if (int.TryParse(hidLessonId.Value, out lessonId) && lessonId > 0)
            {
                Lesson lesson = _lessonBLL.GetLessonDetail(lessonId);
                lesson.LessonTitle = txtLessonTitle.Text.Trim();
                lesson.ContentHTML = txtContentHtml.Text.Trim();
                lesson.VideoUrl = txtVideoUrl.Text.Trim();
                if (newNotesPath != null)
                    lesson.NotesFilePath = newNotesPath;
                _lessonBLL.UpdateLessonContent(lesson);
            }
            else
            {
                _lessonBLL.AddLessonToModule(new Lesson
                {
                    ModuleID = CurrentModuleId,
                    LessonTitle = txtLessonTitle.Text.Trim(),
                    ContentHTML = txtContentHtml.Text.Trim(),
                    VideoUrl = txtVideoUrl.Text.Trim(),
                    NotesFilePath = newNotesPath
                });
            }
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ResetEditor();
        }

        private void ResetEditor()
        {
            hidLessonId.Value = "";
            txtLessonTitle.Text = "";
            txtContentHtml.Text = "";
            txtVideoUrl.Text = "";
            lblCurrentNotes.Text = "";
            lblEditorHeading.InnerText = "New lesson";
            btnCancelEdit.Visible = false;
            lblMessage.Visible = false;
            lblSuccess.Visible = false;
        }

        private void ShowSuccess(string message)
        {
            AdminUi.Success(lblMessage, lblSuccess, message);
        }

        private void ShowError(Exception ex, string context)
        {
            AdminUi.Error(lblMessage, lblSuccess, ex, "The lesson could not be saved. Please try again.", context);
        }
    }
}
