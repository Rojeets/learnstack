using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Data_Access_Layer
{
    public class QuestionDAL
    {
        private const string SelectColumns = "QuestionID, QuizID, QuestionText, Marks";

        public List<Question> SelectByQuizId(int quizId)
        {
            var list = new List<Question>();
            const string sql = "SELECT " + SelectColumns + " FROM Questions " +
                               "WHERE QuizID = @QuizId ORDER BY QuestionID";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@QuizId", SqlDbType.Int, 0, quizId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) list.Add(Map(r));
                }
            }
            return list;
        }

        public int Insert(Question question)
        {
            const string sql =
                "INSERT INTO Questions (QuizID, QuestionText, Marks) " +
                "VALUES (@QuizID, @QuestionText, @Marks); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@QuizID", SqlDbType.Int, 0, question.QuizID);
                DbHelper.AddParam(cmd, "@QuestionText", SqlDbType.NVarChar, 500, question.QuestionText);
                DbHelper.AddParam(cmd, "@Marks", SqlDbType.Int, 0, question.Marks);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public void Update(Question question)
        {
            const string sql =
                "UPDATE Questions SET QuestionText = @QuestionText, Marks = @Marks " +
                "WHERE QuestionID = @QuestionId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@QuestionText", SqlDbType.NVarChar, 500, question.QuestionText);
                DbHelper.AddParam(cmd, "@Marks", SqlDbType.Int, 0, question.Marks);
                DbHelper.AddParam(cmd, "@QuestionId", SqlDbType.Int, 0, question.QuestionID);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int questionId)
        {
            const string sql = "DELETE FROM Questions WHERE QuestionID = @QuestionId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@QuestionId", SqlDbType.Int, 0, questionId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private Question Map(IDataRecord r)
        {
            return new Question
            {
                QuestionID = DbHelper.GetInt(r, "QuestionID"),
                QuizID = DbHelper.GetInt(r, "QuizID"),
                QuestionText = DbHelper.GetString(r, "QuestionText"),
                Marks = DbHelper.GetInt(r, "Marks")
            };
        }
    }
}
