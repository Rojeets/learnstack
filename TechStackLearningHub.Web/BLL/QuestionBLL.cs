using System.Collections.Generic;
using TechStackLearningHub.Data_Access_Layer;
using TechStackLearningHub.Helpers;
using TechStackLearningHub.Models;

namespace TechStackLearningHub.BLL
{
    public class QuestionBLL
    {
        private readonly QuestionDAL _questionDAL = new QuestionDAL();
        private readonly AnswerDAL _answerDAL = new AnswerDAL();

        public List<Question> GetQuestionsByQuizId(int quizId)
        {
            return _questionDAL.SelectByQuizId(quizId);
        }

        public List<Answer> GetAnswerOptions(int questionId)
        {
            return _answerDAL.SelectByQuestionId(questionId);
        }

        public int AddQuestionToQuiz(int quizId, string text, int marks, List<AnswerInput> answers)
        {
            ValidateQuestion(text, marks, answers);

            var question = new Question
            {
                QuizID = quizId,
                QuestionText = text,
                Marks = marks
            };
            int questionId = _questionDAL.Insert(question);
            InsertAnswers(questionId, answers);
            return questionId;
        }

        public void UpdateQuestion(Question question, List<AnswerInput> answers)
        {
            if (question == null)
                throw new ValidationException("Question not supplied.");
            ValidateQuestion(question.QuestionText, question.Marks, answers);

            _questionDAL.Update(question);

            // Full answer-set replace: the quiz builder always resubmits the
            // complete option list, so reconcile by delete + re-insert rather
            // than diffing old vs new options.
            _answerDAL.DeleteByQuestionId(question.QuestionID);
            InsertAnswers(question.QuestionID, answers);
        }

        public void DeleteQuestion(int questionId)
        {
            // Answers are ON DELETE CASCADE from Questions, so the rows go with
            // it; deleting them first would just be a redundant second write.
            _questionDAL.Delete(questionId);
        }

        private void InsertAnswers(int questionId, List<AnswerInput> answers)
        {
            foreach (AnswerInput answer in answers)
            {
                _answerDAL.Insert(new Answer
                {
                    QuestionID = questionId,
                    AnswerText = answer.AnswerText,
                    IsCorrect = answer.IsCorrect
                });
            }
        }

        private void ValidateQuestion(string text, int marks, List<AnswerInput> answers)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ValidationException("Question text is required.");
            if (marks < 1)
                throw new ValidationException("A question must be worth at least 1 mark.");
            if (answers == null || answers.Count < 2)
                throw new ValidationException("A question needs at least two answer options.");

            bool hasCorrect = false;
            foreach (AnswerInput answer in answers)
            {
                if (string.IsNullOrWhiteSpace(answer.AnswerText))
                    throw new ValidationException("Answer text cannot be empty.");
                if (answer.IsCorrect)
                    hasCorrect = true;
            }
            if (!hasCorrect)
                throw new ValidationException("A question must have at least one correct answer.");
        }
    }
}
