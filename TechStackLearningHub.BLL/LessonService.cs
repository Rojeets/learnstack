using System.Data;
using System.Text.RegularExpressions;
using TechStackLearningHub.DAL;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.BLL
{
    public class LessonService
    {
        private readonly LessonRepository _lessonRepository = new LessonRepository();
        private readonly ModuleRepository _moduleRepository = new ModuleRepository();
        private readonly CourseRepository _courseRepository = new CourseRepository();
        private readonly ProgressRepository _progressRepository = new ProgressRepository();

        // Only embeddable player URLs render inside an <iframe>; a full watch
        // page URL would silently show a blank box.
        private static readonly Regex EmbedUrlPattern = new Regex(
            @"^https?://(www\.)?(youtube\.com/embed/|youtu\.be/|player\.vimeo\.com/video/).+$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public DataTable GetLessonsForModule(int moduleId)
        {
            return _lessonRepository.GetLessonsByModuleId(moduleId);
        }

        public Lesson GetLessonDetail(int lessonId)
        {
            return _lessonRepository.GetLessonById(lessonId);
        }

        public int GetCourseIdForLesson(int lessonId)
        {
            Lesson lesson = _lessonRepository.GetLessonById(lessonId);
            if (lesson == null) return 0;
            DAL.Models.Module module = _moduleRepository.GetModuleById(lesson.ModuleID);
            return module == null ? 0 : module.CourseID;
        }

        public bool IsLessonInPublishedCourse(int lessonId)
        {
            int courseId = GetCourseIdForLesson(lessonId);
            if (courseId <= 0) return false;
            Course course = _courseRepository.GetCourseById(courseId);
            return course != null && course.IsPublished;
        }

        public int AddLessonToModule(Lesson lesson)
        {
            DataTable existing = GetLessonsForModule(lesson.ModuleID);
            int nextOrder = 1;
            foreach (DataRow row in existing.Rows)
            {
                int order = (int)row["LessonOrder"];
                if (order >= nextOrder) nextOrder = order + 1;
            }
            lesson.LessonOrder = nextOrder;
            ValidateLesson(lesson);
            return _lessonRepository.InsertLesson(lesson);
        }

        public void UpdateLessonContent(Lesson lesson)
        {
            ValidateLesson(lesson);
            _lessonRepository.UpdateLesson(lesson);
        }

        public void DeleteLesson(int lessonId)
        {
            // Remove orphaned progress rows first so a deleted lesson can't
            // leave phantom "completed" entries in a student's percentage.
            _progressRepository.DeleteProgressByLesson(lessonId);
            _lessonRepository.DeleteLesson(lessonId);
        }

        private void ValidateLesson(Lesson lesson)
        {
            if (string.IsNullOrWhiteSpace(lesson.LessonTitle))
                throw new System.InvalidOperationException("Lesson title is required.");
            if (!string.IsNullOrEmpty(lesson.VideoUrl) && !EmbedUrlPattern.IsMatch(lesson.VideoUrl))
                throw new System.InvalidOperationException(
                    "Video URL must be an embeddable YouTube or Vimeo player URL.");
        }
    }
}