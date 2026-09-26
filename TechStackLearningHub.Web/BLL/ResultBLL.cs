using System;
using System.Collections.Generic;
using TechStackLearningHub.Data_Access_Layer;
using TechStackLearningHub.Models;

namespace TechStackLearningHub.BLL
{
    public class ResultBLL
    {
        private readonly ResultDAL _resultDAL = new ResultDAL();

        public List<QuizStatsRow> GetResultsByQuizId(int quizId)
        {
            return _resultDAL.SelectStatsByQuizId(quizId);
        }

        // One method, one shape. The null-courseId case is the whole-catalogue
        // report, so the caller does not have to know which DAL method backs
        // which filter.
        public List<ResultReportRow> GetAllResultsForReporting(int? courseId)
        {
            return courseId.HasValue
                ? _resultDAL.SelectForReportByCourse(courseId.Value)
                : _resultDAL.SelectAllForReport();
        }

        public int GetAttemptCountSince(DateTime since)
        {
            return _resultDAL.GetAttemptCountSince(since);
        }
    }
}
