using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.DAL
{
    public class QuizRepository
    {
        public Quiz GetQuizById(int quizId)
        {
            const string sql = "SELECT * FROM Quiz WHERE QuizID = @QuizId";
            DataTable table = DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@QuizId", quizId) });
            if (table.Rows.Count == 0) return null;
            return MapRow(table.Rows[0]);
        }

        public Quiz GetQuizByModuleId(int moduleId)
        {
            const string sql = "SELECT * FROM Quiz WHERE ModuleID = @ModuleId";
            DataTable table = DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@ModuleId", moduleId) });
            if (table.Rows.Count == 0) return null;
            return MapRow(table.Rows[0]);
        }

        // Admin quiz picker across every course, not just one module.
        public DataTable GetAllQuizzesForAdmin()
        {
            const string sql = "SELECT q.QuizID, q.QuizTitle, q.PassMarkPercent, m.ModuleTitle, c.CourseName " +
                               "FROM Quiz q " +
                               "JOIN Modules m ON m.ModuleID = q.ModuleID " +
                               "JOIN Courses c ON c.CourseID = m.CourseID " +
                               "ORDER BY c.CourseName, m.ModuleTitle";
            return DbHelper.ExecuteQuery(sql, null);
        }

        public int InsertQuiz(Quiz quiz)
        {
            const string sql = "INSERT INTO Quiz (ModuleID, QuizTitle, PassMarkPercent) " +
                               "VALUES (@ModuleID, @QuizTitle, @PassMarkPercent); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            SqlParameter[] parameters =
            {
                new SqlParameter("@ModuleID", quiz.ModuleID),
                new SqlParameter("@QuizTitle", quiz.QuizTitle),
                new SqlParameter("@PassMarkPercent", quiz.PassMarkPercent)
            };
            return (int)DbHelper.ExecuteScalar(sql, parameters);
        }

        public void UpdateQuiz(Quiz quiz)
        {
            const string sql = "UPDATE Quiz SET QuizTitle = @QuizTitle, PassMarkPercent = @PassMarkPercent " +
                               "WHERE QuizID = @QuizId";
            SqlParameter[] parameters =
            {
                new SqlParameter("@QuizTitle", quiz.QuizTitle),
                new SqlParameter("@PassMarkPercent", quiz.PassMarkPercent),
                new SqlParameter("@QuizId", quiz.QuizID)
            };
            DbHelper.ExecuteNonQuery(sql, parameters);
        }

        public void DeleteQuiz(int quizId)
        {
            const string sql = "DELETE FROM Quiz WHERE QuizID = @QuizId";
            DbHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@QuizId", quizId) });
        }

        private Quiz MapRow(DataRow row)
        {
            return new Quiz
            {
                QuizID = (int)row["QuizID"],
                ModuleID = (int)row["ModuleID"],
                QuizTitle = row["QuizTitle"].ToString(),
                PassMarkPercent = (int)row["PassMarkPercent"]
            };
        }
    }
}