using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Data_Access_Layer
{
    public class ProgressDAL
    {
        public List<Progress> SelectByUserId(int userId)
        {
            var list = new List<Progress>();
            const string sql = "SELECT ProgressID, UserID, LessonID, IsCompleted, CompletionDate " +
                               "FROM Progress WHERE UserID = @UserId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@UserId", SqlDbType.Int, 0, userId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new Progress
                        {
                            ProgressID = DbHelper.GetInt(r, "ProgressID"),
                            UserID = DbHelper.GetInt(r, "UserID"),
                            LessonID = DbHelper.GetInt(r, "LessonID"),
                            IsCompleted = DbHelper.GetBool(r, "IsCompleted"),
                            CompletionDate = DbHelper.GetNullableDate(r, "CompletionDate")
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Whether this user has already completed this lesson. The lesson page
        /// needs the real answer on a fresh GET: the completion button used to
        /// only ever reflect the browser's own optimistic state, so reloading a
        /// finished lesson offered "Mark as Complete" again.
        /// </summary>
        public bool IsLessonComplete(int userId, int lessonId)
        {
            const string sql =
                "SELECT COUNT(1) FROM Progress " +
                "WHERE UserID = @UserId AND LessonID = @LessonId AND IsCompleted = 1";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@UserId", SqlDbType.Int, 0, userId);
                DbHelper.AddParam(cmd, "@LessonId", SqlDbType.Int, 0, lessonId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // Idempotent upsert keyed on the UQ_User_Lesson constraint: calling it twice
        // for the same lesson never creates a duplicate row.
        public void MarkLessonComplete(int userId, int lessonId)
        {
            const string sql =
                "MERGE Progress AS target " +
                "USING (VALUES (@UserId, @LessonId)) AS source (UserID, LessonID) " +
                "ON target.UserID = source.UserID AND target.LessonID = source.LessonID " +
                "WHEN MATCHED THEN UPDATE SET IsCompleted = 1, CompletionDate = GETDATE() " +
                "WHEN NOT MATCHED THEN INSERT (UserID, LessonID, IsCompleted, CompletionDate) " +
                "VALUES (source.UserID, source.LessonID, 1, GETDATE());";
            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@UserId", SqlDbType.Int, 0, userId);
                DbHelper.AddParam(cmd, "@LessonId", SqlDbType.Int, 0, lessonId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Transactional overload used by QuizBLL.SubmitQuizAttempt: all the
        // module's lessons are marked complete inside the quiz's SqlTransaction.
        public void MarkLessonComplete(SqlConnection conn, SqlTransaction tx, int userId, int lessonId)
        {
            const string sql =
                "MERGE Progress AS target " +
                "USING (VALUES (@UserId, @LessonId)) AS source (UserID, LessonID) " +
                "ON target.UserID = source.UserID AND target.LessonID = source.LessonID " +
                "WHEN MATCHED THEN UPDATE SET IsCompleted = 1, CompletionDate = GETDATE() " +
                "WHEN NOT MATCHED THEN INSERT (UserID, LessonID, IsCompleted, CompletionDate) " +
                "VALUES (source.UserID, source.LessonID, 1, GETDATE());";
            SqlParameter[] parameters =
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@LessonId", lessonId)
            };
            DbHelper.ExecuteNonQuery(conn, tx, sql, parameters);
        }

        public void DeleteByLessonId(int lessonId)
        {
            const string sql = "DELETE FROM Progress WHERE LessonID = @LessonId";
            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@LessonId", SqlDbType.Int, 0, lessonId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public decimal GetCourseCompletionPercentage(int userId, int courseId)
        {
            const string sql =
                "SELECT CAST(COUNT(CASE WHEN p.IsCompleted = 1 THEN 1 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) " +
                "FROM Lessons l " +
                "JOIN Modules m ON m.ModuleID = l.ModuleID " +
                "JOIN Courses c ON c.CourseID = m.CourseID " +
                "LEFT JOIN Progress p ON p.LessonID = l.LessonID AND p.UserID = @UserId " +
                "WHERE c.CourseID = @CourseId";
            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@UserId", SqlDbType.Int, 0, userId);
                DbHelper.AddParam(cmd, "@CourseId", SqlDbType.Int, 0, courseId);
                con.Open();
                object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0m : (decimal)value;
            }
        }

        // Dashboard: one summary row per course the user has touched (progress exists).
        public List<CourseProgressSummary> GetCourseProgressSummariesForUser(int userId)
        {
            var list = new List<CourseProgressSummary>();
            const string sql =
                "SELECT c.CourseID, c.CourseName, c.TechStack, " +
                "CAST(COUNT(CASE WHEN p.IsCompleted = 1 THEN 1 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) AS PercentComplete " +
                "FROM Progress p " +
                "JOIN Lessons l ON l.LessonID = p.LessonID " +
                "JOIN Modules m ON m.ModuleID = l.ModuleID " +
                "JOIN Courses c ON c.CourseID = m.CourseID " +
                "WHERE p.UserID = @UserId AND c.IsPublished = 1 " +
                "GROUP BY c.CourseID, c.CourseName, c.TechStack " +
                "ORDER BY c.CourseName";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@UserId", SqlDbType.Int, 0, userId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new CourseProgressSummary
                        {
                            CourseID = DbHelper.GetInt(r, "CourseID"),
                            CourseName = DbHelper.GetString(r, "CourseName"),
                            TechStack = DbHelper.GetString(r, "TechStack"),
                            PercentComplete = DbHelper.GetDecimal(r, "PercentComplete")
                        });
                    }
                }
            }
            return list;
        }

        // Admin Reports: every student's completion % for one course (0% shown
        // for students with no progress, so the report is complete).
        public List<ProgressReportRow> GetProgressSummariesForReporting(int courseId)
        {
            var list = new List<ProgressReportRow>();
            const string sql =
                "SELECT u.Username, c.CourseName, c.TechStack, " +
                "CAST(COUNT(CASE WHEN p.IsCompleted = 1 THEN 1 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) AS PercentComplete " +
                "FROM Users u " +
                "CROSS JOIN Courses c " +
                "JOIN Modules m ON m.CourseID = c.CourseID " +
                "JOIN Lessons l ON l.ModuleID = m.ModuleID " +
                "LEFT JOIN Progress p ON p.LessonID = l.LessonID AND p.UserID = u.UserID " +
                "WHERE c.CourseID = @CourseId " +
                "AND u.RoleID = (SELECT RoleID FROM Roles WHERE RoleName = 'Student') " +
                "GROUP BY u.Username, c.CourseName, c.TechStack " +
                "ORDER BY u.Username";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@CourseId", SqlDbType.Int, 0, courseId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ProgressReportRow
                        {
                            Username = DbHelper.GetString(r, "Username"),
                            CourseName = DbHelper.GetString(r, "CourseName"),
                            TechStack = DbHelper.GetString(r, "TechStack"),
                            PercentComplete = DbHelper.GetDecimal(r, "PercentComplete")
                        });
                    }
                }
            }
            return list;
        }
    }
}
