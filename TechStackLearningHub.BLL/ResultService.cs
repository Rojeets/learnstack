using System;
using System.Data;
using TechStackLearningHub.DAL;

namespace TechStackLearningHub.BLL
{
    public class ResultService
    {
        private readonly ResultRepository _resultRepository = new ResultRepository();

        public DataTable GetResultsByQuizId(int quizId)
        {
            return _resultRepository.GetResultsByQuizId(quizId);
        }

        public DataTable GetAllResultsForReporting(int? courseId)
        {
            return courseId.HasValue
                ? _resultRepository.GetResultsForReportingByCourse(courseId.Value)
                : _resultRepository.GetAllResultsForReporting();
        }

        public int GetAttemptCountSince(DateTime since)
        {
            return _resultRepository.GetAttemptCountSince(since);
        }
    }
}