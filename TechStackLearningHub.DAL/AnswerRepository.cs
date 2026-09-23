using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.DAL
{
    public class AnswerRepository
    {
        public List<Answer> GetAnswersByQuestionId(int questionId)
        {
            const string sql = "SELECT * FROM Answers WHERE QuestionID = @QuestionId";
            DataTable table = DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@QuestionId", questionId) });
            var answers = new List<Answer>();
            foreach (DataRow row in table.Rows)
                answers.Add(MapRow(row));
            return answers;
        }

        public int InsertAnswer(Answer answer)
        {
            const string sql = "INSERT INTO Answers (QuestionID, AnswerText, IsCorrect) " +
                               "VALUES (@QuestionID, @AnswerText, @IsCorrect); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            SqlParameter[] parameters =
            {
                new SqlParameter("@QuestionID", answer.QuestionID),
                new SqlParameter("@AnswerText", answer.AnswerText),
                new SqlParameter("@IsCorrect", answer.IsCorrect)
            };
            return (int)DbHelper.ExecuteScalar(sql, parameters);
        }

        public void UpdateAnswer(Answer answer)
        {
            const string sql = "UPDATE Answers SET AnswerText = @AnswerText, IsCorrect = @IsCorrect " +
                               "WHERE AnswerID = @AnswerId";
            SqlParameter[] parameters =
            {
                new SqlParameter("@AnswerText", answer.AnswerText),
                new SqlParameter("@IsCorrect", answer.IsCorrect),
                new SqlParameter("@AnswerId", answer.AnswerID)
            };
            DbHelper.ExecuteNonQuery(sql, parameters);
        }

        public void DeleteAnswer(int answerId)
        {
            const string sql = "DELETE FROM Answers WHERE AnswerID = @AnswerId";
            DbHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@AnswerId", answerId) });
        }

        private Answer MapRow(DataRow row)
        {
            return new Answer
            {
                AnswerID = (int)row["AnswerID"],
                QuestionID = (int)row["QuestionID"],
                AnswerText = row["AnswerText"].ToString(),
                IsCorrect = (bool)row["IsCorrect"]
            };
        }
    }
}