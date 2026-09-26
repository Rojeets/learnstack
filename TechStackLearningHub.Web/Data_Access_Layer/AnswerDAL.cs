using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.Models;

namespace TechStackLearningHub.Data_Access_Layer
{
    public class AnswerDAL
    {
        private const string SelectColumns = "AnswerID, QuestionID, AnswerText, IsCorrect";

        public List<Answer> SelectByQuestionId(int questionId)
        {
            var list = new List<Answer>();
            const string sql = "SELECT " + SelectColumns + " FROM Answers " +
                               "WHERE QuestionID = @QuestionId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@QuestionId", SqlDbType.Int, 0, questionId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) list.Add(Map(r));
                }
            }
            return list;
        }

        public int Insert(Answer answer)
        {
            const string sql =
                "INSERT INTO Answers (QuestionID, AnswerText, IsCorrect) " +
                "VALUES (@QuestionID, @AnswerText, @IsCorrect); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@QuestionID", SqlDbType.Int, 0, answer.QuestionID);
                DbHelper.AddParam(cmd, "@AnswerText", SqlDbType.NVarChar, 300, answer.AnswerText);
                DbHelper.AddParam(cmd, "@IsCorrect", SqlDbType.Bit, 0, answer.IsCorrect);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public void Update(Answer answer)
        {
            const string sql =
                "UPDATE Answers SET AnswerText = @AnswerText, IsCorrect = @IsCorrect " +
                "WHERE AnswerID = @AnswerId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@AnswerText", SqlDbType.NVarChar, 300, answer.AnswerText);
                DbHelper.AddParam(cmd, "@IsCorrect", SqlDbType.Bit, 0, answer.IsCorrect);
                DbHelper.AddParam(cmd, "@AnswerId", SqlDbType.Int, 0, answer.AnswerID);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int answerId)
        {
            const string sql = "DELETE FROM Answers WHERE AnswerID = @AnswerId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@AnswerId", SqlDbType.Int, 0, answerId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteByQuestionId(int questionId)
        {
            const string sql = "DELETE FROM Answers WHERE QuestionID = @QuestionId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@QuestionId", SqlDbType.Int, 0, questionId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private Answer Map(IDataRecord r)
        {
            return new Answer
            {
                AnswerID = DbHelper.GetInt(r, "AnswerID"),
                QuestionID = DbHelper.GetInt(r, "QuestionID"),
                AnswerText = DbHelper.GetString(r, "AnswerText"),
                IsCorrect = DbHelper.GetBool(r, "IsCorrect")
            };
        }
    }
}
