using System.Data;
using System.Data.SqlClient;

namespace TechStackLearningHub.DAL
{
    public class ProgressRepository
    {
        public DataTable GetProgressByUserId(int userId)
        {
            const string sql = "SELECT * FROM Progress WHERE UserID = @UserId";
            return DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@UserId", userId) });
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
            SqlParameter[] parameters =
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@LessonId", lessonId)
            };
            DbHelper.ExecuteNonQuery(sql, parameters);
        }

        // Transactional overload used by QuizService.SubmitQuizAttempt: all the
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

        public void DeleteProgressByLesson(int lessonId)
        {
            const string sql = "DELETE FROM Progress WHERE LessonID = @LessonId";
            DbHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@LessonId", lessonId) });
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
            SqlParameter[] parameters =
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@CourseId", courseId)
            };
            object value = DbHelper.ExecuteScalar(sql, parameters);
            return value == null || value == System.DBNull.Value ? 0m : (decimal)value;
        }

        // Dashboard: one summary row per course the user has touched (progress exists).
        public DataTable GetCourseProgressSummariesForUser(int userId)
        {
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
            return DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@UserId", userId) });
        }

        // Admin Reports: every student's completion % for one course (0% shown
        // for students with no progress, so the report is complete).
        public DataTable GetProgressSummariesForReporting(int courseId)
        {
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
            return DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@CourseId", courseId) });
        }
    }
}