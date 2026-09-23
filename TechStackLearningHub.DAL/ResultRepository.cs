using System;
using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.DAL
{
    public class ResultRepository
    {
        // Called only from inside DbHelper.ExecuteInTransaction (quiz submission)
        // so the Results insert and the Progress updates share one transaction.
        public int InsertResult(SqlConnection conn, SqlTransaction tx, Result result)
        {
            const string sql = "INSERT INTO Results (UserID, QuizID, Score, AttemptDate, IsPassed) " +
                               "VALUES (@UserId, @QuizId, @Score, @AttemptDate, @IsPassed); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            SqlParameter[] parameters =
            {
                new SqlParameter("@UserId", result.UserID),
                new SqlParameter("@QuizId", result.QuizID),
                new SqlParameter("@Score", result.Score),
                new SqlParameter("@AttemptDate", DateTime.Now),
                new SqlParameter("@IsPassed", result.IsPassed)
            };
            return (int)DbHelper.ExecuteScalar(conn, tx, sql, parameters);
        }

        public Result GetLatestResultByUserAndQuiz(int userId, int quizId)
        {
            const string sql = "SELECT TOP 1 ResultID, UserID, QuizID, Score, AttemptDate, IsPassed " +
                               "FROM Results WHERE UserID = @UserId AND QuizID = @QuizId " +
                               "ORDER BY AttemptDate DESC";
            DataTable table = DbHelper.ExecuteQuery(sql, new SqlParameter[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@QuizId", quizId)
            });
            if (table.Rows.Count == 0)
                return null;

            var row = table.Rows[0];
            return new Result
            {
                ResultID = (int)row["ResultID"],
                UserID = (int)row["UserID"],
                QuizID = (int)row["QuizID"],
                Score = (decimal)row["Score"],
                AttemptDate = (DateTime)row["AttemptDate"],
                IsPassed = (bool)row["IsPassed"]
            };
        }

        public DataTable GetResultsByUserId(int userId)
        {
            const string sql = "SELECT q.QuizTitle, r.Score, r.AttemptDate, r.IsPassed, m.ModuleTitle " +
                               "FROM Results r JOIN Quiz q ON q.QuizID = r.QuizID " +
                               "JOIN Modules m ON m.ModuleID = q.ModuleID " +
                               "WHERE r.UserID = @UserId ORDER BY r.AttemptDate DESC";
            return DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@UserId", userId) });
        }

        public DataTable GetResultsByQuizId(int quizId)
        {
            const string sql = "SELECT q.QuizTitle, COUNT(*) AS AttemptCount, " +
                               "CAST(AVG(r.Score) AS DECIMAL(5,2)) AS AverageScore, " +
                               "CAST(SUM(CASE WHEN r.IsPassed = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) AS PassRate " +
                               "FROM Results r JOIN Quiz q ON q.QuizID = r.QuizID " +
                               "WHERE r.QuizID = @QuizId GROUP BY q.QuizTitle";
            return DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@QuizId", quizId) });
        }

        public DataTable GetAllResultsForReporting()
        {
            const string sql = "SELECT r.ResultID, u.Username, c.CourseName, c.TechStack, m.ModuleTitle, " +
                               "q.QuizTitle, r.Score, r.AttemptDate, r.IsPassed " +
                               "FROM Results r JOIN Users u ON u.UserID = r.UserID " +
                               "JOIN Quiz q ON q.QuizID = r.QuizID " +
                               "JOIN Modules m ON m.ModuleID = q.ModuleID " +
                               "JOIN Courses c ON c.CourseID = m.CourseID " +
                               "ORDER BY r.AttemptDate DESC";
            return DbHelper.ExecuteQuery(sql, null);
        }

        public DataTable GetResultsForReportingByCourse(int courseId)
        {
            const string sql = "SELECT r.ResultID, u.Username, c.CourseName, c.TechStack, m.ModuleTitle, " +
                               "q.QuizTitle, r.Score, r.AttemptDate, r.IsPassed " +
                               "FROM Results r JOIN Users u ON u.UserID = r.UserID " +
                               "JOIN Quiz q ON q.QuizID = r.QuizID " +
                               "JOIN Modules m ON m.ModuleID = q.ModuleID " +
                               "JOIN Courses c ON c.CourseID = m.CourseID " +
                               "WHERE c.CourseID = @CourseId " +
                               "ORDER BY r.AttemptDate DESC";
            return DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@CourseId", courseId) });
        }

        public int GetAttemptCountSince(DateTime since)
        {
            const string sql = "SELECT COUNT(*) FROM Results WHERE AttemptDate >= @Since";
            return (int)DbHelper.ExecuteScalar(sql, new SqlParameter[] { new SqlParameter("@Since", since) });
        }

        public int GetAttemptCountByQuizId(int quizId)
        {
            const string sql = "SELECT COUNT(*) FROM Results WHERE QuizID = @QuizId";
            return (int)DbHelper.ExecuteScalar(sql, new SqlParameter[] { new SqlParameter("@QuizId", quizId) });
        }
    }
}