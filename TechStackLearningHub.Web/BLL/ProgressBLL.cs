using System.Collections.Generic;
using TechStackLearningHub.Web.Data_Access_Layer;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.BLL
{
    public class ProgressBLL
    {
        private readonly ProgressDAL _progressDAL = new ProgressDAL();

        public void MarkLessonComplete(int userId, int lessonId)
        {
            _progressDAL.MarkLessonComplete(userId, lessonId);
        }

        public bool IsLessonComplete(int userId, int lessonId)
        {
            return _progressDAL.IsLessonComplete(userId, lessonId);
        }

        public decimal GetCourseProgressSummary(int userId, int courseId)
        {
            return _progressDAL.GetCourseCompletionPercentage(userId, courseId);
        }

        public List<CourseProgressSummary> GetAllCoursesProgressForUser(int userId)
        {
            return _progressDAL.GetCourseProgressSummariesForUser(userId);
        }

        public List<ProgressReportRow> GetProgressSummariesForReporting(int courseId)
        {
            return _progressDAL.GetProgressSummariesForReporting(courseId);
        }
    }
}
