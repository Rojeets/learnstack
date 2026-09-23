using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TechStackLearningHub.DAL;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.BLL
{
    // Student-facing quiz DTOs. IsCorrect is deliberately absent from every
    // type here so it can never serialise into the HTML sent to the browser.
    public class QuizForStudent
    {
        public int QuizID { get; set; }
        public string QuizTitle { get; set; }
        public int PassMarkPercent { get; set; }
        public List<StudentQuestion> Questions { get; set; } = new List<StudentQuestion>();
    }

    public class StudentQuestion
    {
        public int QuestionID { get; set; }
        public string QuestionText { get; set; }
        public int Marks { get; set; }
        public bool AllowMultiple { get; set; }
        public List<StudentAnswer> Answers { get; set; } = new List<StudentAnswer>();
    }

    public class StudentAnswer
    {
        public int AnswerID { get; set; }
        public string AnswerText { get; set; }
    }

    public class QuizService
    {
        private readonly QuizRepository _quizRepository = new QuizRepository();
        private readonly QuestionRepository _questionRepository = new QuestionRepository();
        private readonly AnswerRepository _answerRepository = new AnswerRepository();
        private readonly ResultRepository _resultRepository = new ResultRepository();
        private readonly ProgressRepository _progressRepository = new ProgressRepository();
        private readonly LessonRepository _lessonRepository = new LessonRepository();
        private readonly ModuleRepository _moduleRepository = new ModuleRepository();
        private readonly CourseRepository _courseRepository = new CourseRepository();

        // Student-facing entry point: returns the quiz only when its module
        // belongs to a published course, so a hand-typed ModuleID can never
        // expose a draft course's quiz.
        public QuizForStudent GetQuizForStudentIfPublished(int moduleId)
        {
            if (!IsModuleInPublishedCourse(moduleId))
                return null;
            return GetQuizForStudent(moduleId);
        }

        public bool IsModuleInPublishedCourse(int moduleId)
        {
            Quiz quiz = _quizRepository.GetQuizByModuleId(moduleId);
            if (quiz == null) return false;
            DAL.Models.Module module = _moduleRepository.GetModuleById(moduleId);
            if (module == null) return false;
            Course course = _courseRepository.GetCourseById(module.CourseID);
            return course != null && course.IsPublished;
        }

        public Quiz GetQuizForModuleId(int moduleId)
        {
            return _quizRepository.GetQuizByModuleId(moduleId);
        }

        public QuizForStudent GetQuizForStudent(int moduleId)
        {
            Quiz quiz = _quizRepository.GetQuizByModuleId(moduleId);
            if (quiz == null)
                return null;

            var model = new QuizForStudent
            {
                QuizID = quiz.QuizID,
                QuizTitle = quiz.QuizTitle,
                PassMarkPercent = quiz.PassMarkPercent
            };

            foreach (var q in _questionRepository.GetQuestionsByQuizId(quiz.QuizID))
            {
                List<Answer> answers = _answerRepository.GetAnswersByQuestionId(q.QuestionID);
                int correctCount = answers.Count(a => a.IsCorrect);

                var studentQuestion = new StudentQuestion
                {
                    QuestionID = q.QuestionID,
                    QuestionText = q.QuestionText,
                    Marks = q.Marks,
                    AllowMultiple = correctCount > 1
                };
                foreach (var a in answers)
                {
                    studentQuestion.Answers.Add(new StudentAnswer
                    {
                        AnswerID = a.AnswerID,
                        AnswerText = a.AnswerText
                    });
                }
                model.Questions.Add(studentQuestion);
            }
            return model;
        }

        public Result SubmitQuizAttempt(int userId, int quizId, IDictionary<int, List<int>> selectedAnswerIdsByQuestionId)
        {
            if (selectedAnswerIdsByQuestionId == null)
                selectedAnswerIdsByQuestionId = new Dictionary<int, List<int>>();

            // Server-side retake guard: even if a kept quiz postback is replayed,
            // a passed quiz can never be submitted again.
            if (HasPassedQuiz(userId, quizId))
                throw new InvalidOperationException("This quiz has already been passed and cannot be retaken.");

            Result result = null;
            // The Results insert and the Progress updates must be atomic: a
            // passing score with no progress credit is a half-written state.
            DbHelper.ExecuteInTransaction((conn, tx) =>
            {
                Quiz quiz = _quizRepository.GetQuizById(quizId);
                if (quiz == null)
                    throw new InvalidOperationException("Quiz not found.");

                decimal earnedMarks = 0m;
                decimal totalMarks = 0m;
                foreach (var question in _questionRepository.GetQuestionsByQuizId(quizId))
                {
                    totalMarks += question.Marks;

                    // Fresh DB read — never trust any "correct" data posted by
                    // the client, only which answer ids were selected.
                    var correctIds = new HashSet<int>(
                        _answerRepository.GetAnswersByQuestionId(question.QuestionID)
                            .Where(a => a.IsCorrect)
                            .Select(a => a.AnswerID));

                    List<int> selected;
                    if (!selectedAnswerIdsByQuestionId.TryGetValue(question.QuestionID, out selected))
                        selected = new List<int>();

                    // Exact set match handles single- and multi-answer questions
                    // with one comparison.
                    if (correctIds.SetEquals(selected))
                        earnedMarks += question.Marks;
                }

                decimal scorePercent = totalMarks == 0
                    ? 0m
                    : Math.Round(earnedMarks * 100m / totalMarks, 2);

                var attempt = new Result
                {
                    UserID = userId,
                    QuizID = quizId,
                    Score = scorePercent,
                    IsPassed = scorePercent >= quiz.PassMarkPercent
                };
                attempt.ResultID = _resultRepository.InsertResult(conn, tx, attempt);

                // Passing the quiz marks every lesson in the module complete.
                DataTable lessons = _lessonRepository.GetLessonsByModuleId(quiz.ModuleID);
                foreach (DataRow row in lessons.Rows)
                {
                    _progressRepository.MarkLessonComplete(conn, tx, userId, (int)row["LessonID"]);
                }

                result = attempt;
            });

            return result;
        }

        public DataTable GetQuizHistoryForUser(int userId)
        {
            return _resultRepository.GetResultsByUserId(userId);
        }

        // A quiz may only ever be taken to pass once; a passed attempt locks it.
        public bool HasPassedQuiz(int userId, int quizId)
        {
            Result latest = _resultRepository.GetLatestResultByUserAndQuiz(userId, quizId);
            return latest != null && latest.IsPassed;
        }

        public int GetModuleCourseId(int moduleId)
        {
            DAL.Models.Module module = _moduleRepository.GetModuleById(moduleId);
            return module == null ? 0 : module.CourseID;
        }

        // ---- Admin quiz management -----------------------------------------

        public DataTable GetAllQuizzesForAdmin()
        {
            return _quizRepository.GetAllQuizzesForAdmin();
        }

        public int CreateQuiz(int moduleId, string title, int passMarkPercent)
        {
            ValidateQuiz(title, passMarkPercent);
            if (_quizRepository.GetQuizByModuleId(moduleId) != null)
                throw new InvalidOperationException("This module already has a quiz.");
            return _quizRepository.InsertQuiz(new Quiz
            {
                ModuleID = moduleId,
                QuizTitle = title,
                PassMarkPercent = passMarkPercent
            });
        }

        public void UpdateQuiz(int quizId, string title, int passMarkPercent)
        {
            ValidateQuiz(title, passMarkPercent);
            _quizRepository.UpdateQuiz(new Quiz
            {
                QuizID = quizId,
                ModuleID = 0,
                QuizTitle = title,
                PassMarkPercent = passMarkPercent
            });
        }

        public void DeleteQuiz(int quizId)
        {
            if (_resultRepository.GetAttemptCountByQuizId(quizId) > 0)
                throw new InvalidOperationException("This quiz already has attempts and cannot be deleted.");

            foreach (var question in _questionRepository.GetQuestionsByQuizId(quizId))
            {
                _answerRepository.DeleteAnswersByQuestionId(question.QuestionID);
                _questionRepository.DeleteQuestion(question.QuestionID);
            }
            _quizRepository.DeleteQuiz(quizId);
        }

        private void ValidateQuiz(string title, int passMarkPercent)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new InvalidOperationException("Quiz title is required.");
            if (passMarkPercent < 0 || passMarkPercent > 100)
                throw new InvalidOperationException("Pass mark must be between 0 and 100.");
        }
    }
}