using System.Data;
using TechStackLearningHub.DAL;

namespace TechStackLearningHub.BLL
{
    public class ProgressService
    {
        private readonly ProgressRepository _progressRepository = new ProgressRepository();

        public void MarkLessonComplete(int userId, int lessonId)
        {
            _progressRepository.MarkLessonComplete(userId, lessonId);
        }

        public decimal GetCourseProgressSummary(int userId, int courseId)
        {
            return _progressRepository.GetCourseCompletionPercentage(userId, courseId);
        }

        public DataTable GetAllCoursesProgressForUser(int userId)
        {
            return _progressRepository.GetCourseProgressSummariesForUser(userId);
        }

        public DataTable GetProgressSummariesForReporting(int courseId)
        {
            return _progressRepository.GetProgressSummariesForReporting(courseId);
        }
    }
}