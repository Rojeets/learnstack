using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.Models;

namespace TechStackLearningHub.Data_Access_Layer
{
    public class QuizDAL
    {
        private const string SelectColumns = "QuizID, ModuleID, QuizTitle, PassMarkPercent";

        public Quiz SelectById(int quizId)
        {
            const string sql = "SELECT " + SelectColumns + " FROM Quiz WHERE QuizID = @QuizId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@QuizId", SqlDbType.Int, 0, quizId);
                con.Open();
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return r.Read() ? Map(r) : null;
                }
            }
        }

        public Quiz SelectByModuleId(int moduleId)
        {
            const string sql = "SELECT " + SelectColumns + " FROM Quiz WHERE ModuleID = @ModuleId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@ModuleId", SqlDbType.Int, 0, moduleId);
                con.Open();
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return r.Read() ? Map(r) : null;
                }
            }
        }

        /// <summary>
        /// Admin picker spanning every course and module, so the admin can
        /// choose any quiz to edit. Returns a flat join shape, not a Quiz,
        /// because the picker displays ModuleTitle and CourseName.
        /// </summary>
        public List<QuizListItem> SelectAllForAdmin()
        {
            var list = new List<QuizListItem>();
            const string sql =
                "SELECT q.QuizID, q.QuizTitle, q.PassMarkPercent, m.ModuleTitle, c.CourseName " +
                "FROM Quiz q " +
                "JOIN Modules m ON m.ModuleID = q.ModuleID " +
                "JOIN Courses c ON c.CourseID = m.CourseID " +
                "ORDER BY c.CourseName, m.ModuleTitle";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new QuizListItem
                        {
                            QuizID = DbHelper.GetInt(r, "QuizID"),
                            QuizTitle = DbHelper.GetString(r, "QuizTitle"),
                            PassMarkPercent = DbHelper.GetInt(r, "PassMarkPercent"),
                            ModuleTitle = DbHelper.GetString(r, "ModuleTitle"),
                            CourseName = DbHelper.GetString(r, "CourseName")
                        });
                    }
                }
            }
            return list;
        }

        public int Insert(Quiz quiz)
        {
            const string sql =
                "INSERT INTO Quiz (ModuleID, QuizTitle, PassMarkPercent) " +
                "VALUES (@ModuleID, @QuizTitle, @PassMarkPercent); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@ModuleID", SqlDbType.Int, 0, quiz.ModuleID);
                DbHelper.AddParam(cmd, "@QuizTitle", SqlDbType.NVarChar, 150, quiz.QuizTitle);
                DbHelper.AddParam(cmd, "@PassMarkPercent", SqlDbType.Int, 0, quiz.PassMarkPercent);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public void Update(Quiz quiz)
        {
            const string sql =
                "UPDATE Quiz SET QuizTitle = @QuizTitle, PassMarkPercent = @PassMarkPercent " +
                "WHERE QuizID = @QuizId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@QuizTitle", SqlDbType.NVarChar, 150, quiz.QuizTitle);
                DbHelper.AddParam(cmd, "@PassMarkPercent", SqlDbType.Int, 0, quiz.PassMarkPercent);
                DbHelper.AddParam(cmd, "@QuizId", SqlDbType.Int, 0, quiz.QuizID);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int quizId)
        {
            const string sql = "DELETE FROM Quiz WHERE QuizID = @QuizId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@QuizId", SqlDbType.Int, 0, quizId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private Quiz Map(IDataRecord r)
        {
            return new Quiz
            {
                QuizID = DbHelper.GetInt(r, "QuizID"),
                ModuleID = DbHelper.GetInt(r, "ModuleID"),
                QuizTitle = DbHelper.GetString(r, "QuizTitle"),
                PassMarkPercent = DbHelper.GetInt(r, "PassMarkPercent")
            };
        }
    }
}
