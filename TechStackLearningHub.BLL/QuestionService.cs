using System.Collections.Generic;
using TechStackLearningHub.DAL;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.BLL
{
    public class QuestionService
    {
        private readonly QuestionRepository _questionRepository = new QuestionRepository();
        private readonly AnswerRepository _answerRepository = new AnswerRepository();

        public List<Question> GetQuestionsByQuizId(int quizId)
        {
            return _questionRepository.GetQuestionsByQuizId(quizId);
        }

        public int AddQuestionToQuiz(int quizId, string text, int marks, List<AnswerInput> answers)
        {
            RequireAtLeastOneCorrect(answers);

            var question = new Question
            {
                QuizID = quizId,
                QuestionText = text,
                Marks = marks
            };
            int questionId = _questionRepository.InsertQuestion(question);
            foreach (var answer in answers)
            {
                _answerRepository.InsertAnswer(new Answer
                {
                    QuestionID = questionId,
                    AnswerText = answer.AnswerText,
                    IsCorrect = answer.IsCorrect
                });
            }
            return questionId;
        }

        public void UpdateQuestion(Question question, List<AnswerInput> answers)
        {
            RequireAtLeastOneCorrect(answers);

            _questionRepository.UpdateQuestion(question);

            // Full answer-set replace: the quiz builder always resubmits the
            // complete option list, so reconcile by delete + re-insert rather
            // than diffing old vs new options.
            _answerRepository.DeleteAnswersByQuestionId(question.QuestionID);
            foreach (var answer in answers)
            {
                _answerRepository.InsertAnswer(new Answer
                {
                    QuestionID = question.QuestionID,
                    AnswerText = answer.AnswerText,
                    IsCorrect = answer.IsCorrect
                });
            }
        }

        public void DeleteQuestion(int questionId)
        {
            _questionRepository.DeleteQuestion(questionId);
        }

        private void RequireAtLeastOneCorrect(List<AnswerInput> answers)
        {
            if (answers == null || answers.Count < 2)
                throw new System.InvalidOperationException("A question needs at least two answer options.");
            bool hasCorrect = false;
            foreach (var answer in answers)
            {
                if (string.IsNullOrWhiteSpace(answer.AnswerText))
                    throw new System.InvalidOperationException("Answer text cannot be empty.");
                if (answer.IsCorrect)
                    hasCorrect = true;
            }
            if (!hasCorrect)
                throw new System.InvalidOperationException("A question must have at least one correct answer.");
        }
    }
}