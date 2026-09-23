using System.Data;
using System.Text.RegularExpressions;
using TechStackLearningHub.DAL;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.BLL
{
    public class LessonService
    {
        private readonly LessonRepository _lessonRepository = new LessonRepository();
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