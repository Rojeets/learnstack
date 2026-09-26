using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Data_Access_Layer
{
    public class ResultDAL
    {
        // ------------------------------------------------------------------
        // Transactional insert.
        //
        // Takes the caller's connection and transaction rather than opening
        // its own, so the Results row joins the same transaction as the
        // Progress updates in QuizBLL.SubmitQuizAttempt. Opening a second
        // connection here would silently write OUTSIDE that transaction.
        //
        // The reader-safe rule: materialise results fully inside the using
        // blocks and never let a SqlDataReader outlive this call, or the
        // enclosing transaction's later writes can deadlock against it.
        // ------------------------------------------------------------------
        public int Insert(SqlConnection conn, SqlTransaction tx, Result result)
        {
            const string sql =
                "INSERT INTO Results (UserID, QuizID, Score, AttemptDate, IsPassed) " +
                "VALUES (@UserId, @QuizId, @Score, @AttemptDate, @IsPassed); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return (int)DbHelper.ExecuteScalar(conn, tx, sql, new SqlParameter[]
            {
                new SqlParameter("@UserId", SqlDbType.Int, 0) { Value = result.UserID },
                new SqlParameter("@QuizId", SqlDbType.Int, 0) { Value = result.QuizID },
                new SqlParameter("@Score", SqlDbType.Decimal, 5) { Precision = 5, Scale = 2, Value = result.Score },
                new SqlParameter("@AttemptDate", SqlDbType.DateTime, 0) { Value = DateTime.Now },
                new SqlParameter("@IsPassed", SqlDbType.Bit, 0) { Value = result.IsPassed }
            });
        }

        public Result SelectLatestByUserAndQuiz(int userId, int quizId)
        {
            const string sql =
                "SELECT TOP 1 ResultID, UserID, QuizID, Score, AttemptDate, IsPassed " +
                "FROM Results WHERE UserID = @UserId AND QuizID = @QuizId " +
                "ORDER BY AttemptDate DESC";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@UserId", SqlDbType.Int, 0, userId);
                DbHelper.AddParam(cmd, "@QuizId", SqlDbType.Int, 0, quizId);
                con.Open();
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return r.Read() ? MapResult(r) : null;
                }
            }
        }

        public List<AttemptSummary> SelectByUserId(int userId)
        {
            var list = new List<AttemptSummary>();
            const string sql =
                "SELECT q.QuizTitle, r.Score, r.AttemptDate, r.IsPassed, m.ModuleTitle " +
                "FROM Results r JOIN Quiz q ON q.QuizID = r.QuizID " +
                "JOIN Modules m ON m.ModuleID = q.ModuleID " +
                "WHERE r.UserID = @UserId ORDER BY r.AttemptDate DESC";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@UserId", SqlDbType.Int, 0, userId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new AttemptSummary
                        {
                            QuizTitle = DbHelper.GetString(r, "QuizTitle"),
                            Score = DbHelper.GetDecimal(r, "Score"),
                            AttemptDate = DbHelper.GetDate(r, "AttemptDate"),
                            IsPassed = DbHelper.GetBool(r, "IsPassed"),
                            ModuleTitle = DbHelper.GetString(r, "ModuleTitle")
                        });
                    }
                }
            }
            return list;
        }

        public List<QuizStatsRow> SelectStatsByQuizId(int quizId)
        {
            var list = new List<QuizStatsRow>();
            const string sql =
                "SELECT q.QuizTitle, COUNT(*) AS AttemptCount, " +
                "CAST(AVG(r.Score) AS DECIMAL(5,2)) AS AverageScore, " +
                "CAST(SUM(CASE WHEN r.IsPassed = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) AS PassRate " +
                "FROM Results r JOIN Quiz q ON q.QuizID = r.QuizID " +
                "WHERE r.QuizID = @QuizId GROUP BY q.QuizTitle";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@QuizId", SqlDbType.Int, 0, quizId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new QuizStatsRow
                        {
                            QuizTitle = DbHelper.GetString(r, "QuizTitle"),
                            AttemptCount = DbHelper.GetInt(r, "AttemptCount"),
                            AverageScore = DbHelper.GetDecimal(r, "AverageScore"),
                            PassRate = DbHelper.GetDecimal(r, "PassRate")
                        });
                    }
                }
            }
            return list;
        }

        public List<ResultReportRow> SelectAllForReport()
        {
            return Report(null);
        }

        public List<ResultReportRow> SelectForReportByCourse(int courseId)
        {
            return Report(courseId);
        }

        /// <summary>
        /// One query serves both the unfiltered report and the per-course
        /// report: the only difference is the course predicate, so the shared
        /// (@CourseID IS NULL OR c.CourseID = @CourseID) form keeps a single
        /// projection to maintain and no string concatenation - hence no
        /// injection surface.
        /// </summary>
        private List<ResultReportRow> Report(int? courseId)
        {
            var list = new List<ResultReportRow>();
            const string sql =
                "SELECT r.ResultID, u.Username, c.CourseName, c.TechStack, m.ModuleTitle, " +
                "q.QuizTitle, r.Score, r.AttemptDate, r.IsPassed " +
                "FROM Results r JOIN Users u ON u.UserID = r.UserID " +
                "JOIN Quiz q ON q.QuizID = r.QuizID " +
                "JOIN Modules m ON m.ModuleID = q.ModuleID " +
                "JOIN Courses c ON c.CourseID = m.CourseID " +
                "WHERE (@CourseID IS NULL OR c.CourseID = @CourseID) " +
                "ORDER BY r.AttemptDate DESC";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@CourseID", SqlDbType.Int, 0,
                    courseId.HasValue ? (object)courseId.Value : null);
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ResultReportRow
                        {
                            ResultID = DbHelper.GetInt(r, "ResultID"),
                            Username = DbHelper.GetString(r, "Username"),
                            CourseName = DbHelper.GetString(r, "CourseName"),
                            TechStack = DbHelper.GetString(r, "TechStack"),
                            ModuleTitle = DbHelper.GetString(r, "ModuleTitle"),
                            QuizTitle = DbHelper.GetString(r, "QuizTitle"),
                            Score = DbHelper.GetDecimal(r, "Score"),
                            AttemptDate = DbHelper.GetDate(r, "AttemptDate"),
                            IsPassed = DbHelper.GetBool(r, "IsPassed")
                        });
                    }
                }
            }
            return list;
        }

        public int GetAttemptCountSince(DateTime since)
        {
            const string sql = "SELECT COUNT(*) FROM Results WHERE AttemptDate >= @Since";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@Since", SqlDbType.DateTime, 0, since);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public int GetAttemptCountByQuizId(int quizId)
        {
            const string sql = "SELECT COUNT(*) FROM Results WHERE QuizID = @QuizId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@QuizId", SqlDbType.Int, 0, quizId);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        private Result MapResult(IDataRecord r)
        {
            return new Result
            {
                ResultID = DbHelper.GetInt(r, "ResultID"),
                UserID = DbHelper.GetInt(r, "UserID"),
                QuizID = DbHelper.GetInt(r, "QuizID"),
                Score = DbHelper.GetDecimal(r, "Score"),
                AttemptDate = DbHelper.GetDate(r, "AttemptDate"),
                IsPassed = DbHelper.GetBool(r, "IsPassed")
            };
        }
    }
}
