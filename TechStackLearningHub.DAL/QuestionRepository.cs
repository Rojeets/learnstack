using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.DAL
{
    public class QuestionRepository
    {
        public List<Question> GetQuestionsByQuizId(int quizId)
        {
            const string sql = "SELECT * FROM Questions WHERE QuizID = @QuizId ORDER BY QuestionID";
            DataTable table = DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@QuizId", quizId) });
            var questions = new List<Question>();
            foreach (DataRow row in table.Rows)
                questions.Add(MapRow(row));
            return questions;
        }

        public int InsertQuestion(Question question)
        {
            const string sql = "INSERT INTO Questions (QuizID, QuestionText, Marks) " +
                               "VALUES (@QuizID, @QuestionText, @Marks); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            SqlParameter[] parameters =
            {
                new SqlParameter("@QuizID", question.QuizID),
                new SqlParameter("@QuestionText", question.QuestionText),
                new SqlParameter("@Marks", question.Marks)
            };
            return (int)DbHelper.ExecuteScalar(sql, parameters);
        }

        public void UpdateQuestion(Question question)
        {
            const string sql = "UPDATE Questions SET QuestionText = @QuestionText, Marks = @Marks " +
                               "WHERE QuestionID = @QuestionId";
            SqlParameter[] parameters =
            {
                new SqlParameter("@QuestionText", question.QuestionText),
                new SqlParameter("@Marks", question.Marks),
                new SqlParameter("@QuestionId", question.QuestionID)
            };
            DbHelper.ExecuteNonQuery(sql, parameters);
        }

        public void DeleteQuestion(int questionId)
        {
            const string sql = "DELETE FROM Questions WHERE QuestionID = @QuestionId";
            DbHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@QuestionId", questionId) });
        }

        private Question MapRow(DataRow row)
        {
            return new Question
            {
                QuestionID = (int)row["QuestionID"],
                QuizID = (int)row["QuizID"],
                QuestionText = row["QuestionText"].ToString(),
                Marks = (int)row["Marks"]
            };
        }
    }
}