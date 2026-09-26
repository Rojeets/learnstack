using System.Collections.Generic;
using System.Text.RegularExpressions;
using TechStackLearningHub.Data_Access_Layer;
using TechStackLearningHub.Helpers;
using TechStackLearningHub.Models;

namespace TechStackLearningHub.BLL
{
    public class LessonBLL
    {
        private readonly LessonDAL _lessonDAL = new LessonDAL();
        private readonly ModuleDAL _moduleDAL = new ModuleDAL();
        private readonly CourseDAL _courseDAL = new CourseDAL();
        private readonly ProgressDAL _progressDAL = new ProgressDAL();

        // Only embeddable player URLs render inside an <iframe>; a full watch
        // page URL would silently show a blank box.
        private static readonly Regex EmbedUrlPattern = new Regex(
            @"^https?://(www\.)?(youtube\.com/embed/|youtu\.be/|player\.vimeo\.com/video/).+$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public List<Lesson> GetLessonsForModule(int moduleId)
        {
            return _lessonDAL.SelectByModuleId(moduleId);
        }

        public Lesson GetLessonDetail(int lessonId)
        {
            return _lessonDAL.SelectById(lessonId);
        }

        public int GetCourseIdForLesson(int lessonId)
        {
            Lesson lesson = _lessonDAL.SelectById(lessonId);
            if (lesson == null) return 0;
            Module module = _moduleDAL.SelectById(lesson.ModuleID);
            return module == null ? 0 : module.CourseID;
        }

        public bool IsLessonInPublishedCourse(int lessonId)
        {
            int courseId = GetCourseIdForLesson(lessonId);
            if (courseId <= 0) return false;
            Course course = _courseDAL.SelectById(courseId);
            return course != null && course.IsPublished;
        }

        public int AddLessonToModule(Lesson lesson)
        {
            List<Lesson> existing = GetLessonsForModule(lesson.ModuleID);
            int nextOrder = 1;
            foreach (Lesson sibling in existing)
            {
                if (sibling.LessonOrder >= nextOrder) nextOrder = sibling.LessonOrder + 1;
            }
            lesson.LessonOrder = nextOrder;
            ValidateLesson(lesson);
            return _lessonDAL.Insert(lesson);
        }

        public void UpdateLessonContent(Lesson lesson)
        {
            ValidateLesson(lesson);
            _lessonDAL.Update(lesson);
        }

        public void DeleteLesson(int lessonId)
        {
            // Remove orphaned progress rows first so a deleted lesson can't
            // leave phantom "completed" entries in a student's percentage.
            _progressDAL.DeleteByLessonId(lessonId);
            _lessonDAL.Delete(lessonId);
        }

        private void ValidateLesson(Lesson lesson)
        {
            if (lesson == null)
                throw new ValidationException("Lesson not supplied.");

            if (string.IsNullOrWhiteSpace(lesson.LessonTitle))
                throw new ValidationException("Lesson title is required.");

            if (!string.IsNullOrEmpty(lesson.VideoUrl) && !EmbedUrlPattern.IsMatch(lesson.VideoUrl))
                throw new ValidationException(
                    "Video URL must be an embeddable YouTube or Vimeo player URL.");
        }
    }
}
