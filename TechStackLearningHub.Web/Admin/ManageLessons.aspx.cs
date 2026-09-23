using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class ManageLessons : Page
    {
        private readonly CourseService _courseService = new CourseService();
        private readonly ModuleService _moduleService = new ModuleService();
        private readonly LessonService _lessonService = new LessonService();

        private static readonly string[] AllowedNoteExtensions = { ".pdf", ".docx", ".doc", ".txt" };
        private const long MaxNoteBytes = 5 * 1024 * 1024;

        protected void Page_Load(object sender, EventArgs e)
        {
            RoleGuard.RequireAdmin(this);

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
            foreach (DataRow row in _courseService.GetAllCoursesForAdmin().Rows)
            {
                ddlCourse.Items.Add(new ListItem(
                    string.Format("{0} ({1})", row["CourseName"], row["TechStack"]),
                    row["CourseID"].ToString()));
            }

            int courseId;
            if (int.TryParse(Request.QueryString["CourseID"], out courseId))
            {
                ListItem item = ddlCourse.Items.FindByValue(courseId.ToString());
                if (item != null)
                    ddlCourse.SelectedValue = courseId.ToString();
            }

            BindModuleOptions();

            int moduleId;
            if (int.TryParse(Request.QueryString["ModuleID"], out moduleId))
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
                foreach (DataRow row in _moduleService.GetModulesForCourse(courseId).Rows)
                {
                    ddlModule.Items.Add(new ListItem(row["ModuleTitle"].ToString(), row["ModuleID"].ToString()));
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

        private void LoadWorkspace()
        {
            if (CurrentModuleId <= 0)
            {
                pnlWorkspace.Visible = false;
                return;
            }
            pnlWorkspace.Visible = true;
            grdLessons.DataSource = _lessonService.GetLessonsForModule(CurrentModuleId);
            grdLessons.DataBind();
        }

        protected void grdLessons_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int lessonId;
            if (!int.TryParse(e.CommandArgument as string, out lessonId))
                return;

            switch (e.CommandName)
            {
                case "EditLesson":
                    BeginEdit(lessonId);
                    break;
                case "DeleteLesson":
                    _lessonService.DeleteLesson(lessonId);
                    ResetEditor();
                    LoadWorkspace();
                    break;
            }
        }

        private void BeginEdit(int lessonId)
        {
            Lesson lesson = _lessonService.GetLessonDetail(lessonId);
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
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string notesPath = SaveUploadedNotes();
                ApplyLesson(notesPath);
                ResetEditor();
                LoadWorkspace();
            }
            catch (Exception ex)
            {
                lblMessage.Text = HttpUtility.HtmlEncode(ex.Message);
                lblMessage.Visible = true;
            }
        }

        private string SaveUploadedNotes()
        {
            if (!fupNotes.HasFile)
                return null;

            string extension = Path.GetExtension(fupNotes.FileName).ToLowerInvariant();
            if (Array.IndexOf(AllowedNoteExtensions, extension) < 0)
                throw new InvalidOperationException("Notes file must be a PDF, DOCX, DOC or TXT file.");
            if (fupNotes.PostedFile.ContentLength > MaxNoteBytes)
                throw new InvalidOperationException("Notes file must be 5 MB or smaller.");

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
                Lesson lesson = _lessonService.GetLessonDetail(lessonId);
                lesson.LessonTitle = txtLessonTitle.Text.Trim();
                lesson.ContentHTML = txtContentHtml.Text.Trim();
                lesson.VideoUrl = txtVideoUrl.Text.Trim();
                if (newNotesPath != null)
                    lesson.NotesFilePath = newNotesPath;
                _lessonService.UpdateLessonContent(lesson);
            }
            else
            {
                _lessonService.AddLessonToModule(new Lesson
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
        }
    }
}