using System;
using System.Collections.Generic;
using System.Linq;
using TechStackLearningHub.Data_Access_Layer;
using TechStackLearningHub.Helpers;
using TechStackLearningHub.Models;

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

    public class QuizBLL
    {
        private readonly QuizDAL _quizDAL = new QuizDAL();
        private readonly QuestionDAL _questionDAL = new QuestionDAL();
        private readonly AnswerDAL _answerDAL = new AnswerDAL();
        private readonly ResultDAL _resultDAL = new ResultDAL();
        private readonly ProgressDAL _progressDAL = new ProgressDAL();
        private readonly LessonDAL _lessonDAL = new LessonDAL();
        private readonly ModuleDAL _moduleDAL = new ModuleDAL();
        private readonly CourseDAL _courseDAL = new CourseDAL();

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
            Quiz quiz = _quizDAL.SelectByModuleId(moduleId);
            if (quiz == null) return false;
            Module module = _moduleDAL.SelectById(moduleId);
            if (module == null) return false;
            Course course = _courseDAL.SelectById(module.CourseID);
            return course != null && course.IsPublished;
        }

        public Quiz GetQuizForModuleId(int moduleId)
        {
            return _quizDAL.SelectByModuleId(moduleId);
        }

        public QuizForStudent GetQuizForStudent(int moduleId)
        {
            Quiz quiz = _quizDAL.SelectByModuleId(moduleId);
            if (quiz == null)
                return null;

            var model = new QuizForStudent
            {
                QuizID = quiz.QuizID,
                QuizTitle = quiz.QuizTitle,
                PassMarkPercent = quiz.PassMarkPercent
            };

            foreach (Question q in _questionDAL.SelectByQuizId(quiz.QuizID))
            {
                List<Answer> answers = _answerDAL.SelectByQuestionId(q.QuestionID);
                int correctCount = answers.Count(a => a.IsCorrect);

                var studentQuestion = new StudentQuestion
                {
                    QuestionID = q.QuestionID,
                    QuestionText = q.QuestionText,
                    Marks = q.Marks,
                    AllowMultiple = correctCount > 1
                };
                foreach (Answer a in answers)
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
                throw new ValidationException("This quiz has already been passed and cannot be retaken.");

            Result result = null;
            // The Results insert and the Progress updates must be atomic: a
            // passing score with no progress credit is a half-written state.
            DbHelper.ExecuteInTransaction((conn, tx) =>
            {
                Quiz quiz = _quizDAL.SelectById(quizId);
                if (quiz == null)
                    throw new ValidationException("Quiz not found.");

                decimal earnedMarks = 0m;
                decimal totalMarks = 0m;
                foreach (Question question in _questionDAL.SelectByQuizId(quizId))
                {
                    totalMarks += question.Marks;

                    // Fresh DB read — never trust any "correct" data posted by
                    // the client, only which answer ids were selected.
                    var correctIds = new HashSet<int>(
                        _answerDAL.SelectByQuestionId(question.QuestionID)
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
                attempt.ResultID = _resultDAL.Insert(conn, tx, attempt);

                // Passing the quiz marks every lesson in the module complete.
                // These reads run on their own short-lived connections BEFORE
                // any write on the transaction's connection, so the progress
                // MERGE that follows can never be blocked by an open reader.
                // Reading the lesson ids first and only then writing keeps the
                // transaction free of MARS/reader-collision problems.
                List<int> lessonIds = _lessonDAL.SelectByModuleId(quiz.ModuleID)
                    .Select(l => l.LessonID)
                    .ToList();
                foreach (int lessonId in lessonIds)
                {
                    _progressDAL.MarkLessonComplete(conn, tx, userId, lessonId);
                }

                result = attempt;
            });

            return result;
        }

        public List<AttemptSummary> GetQuizHistoryForUser(int userId)
        {
            return _resultDAL.SelectByUserId(userId);
        }

        // A quiz may only ever be taken to pass once; a passed attempt locks it.
        public bool HasPassedQuiz(int userId, int quizId)
        {
            Result latest = _resultDAL.SelectLatestByUserAndQuiz(userId, quizId);
            return latest != null && latest.IsPassed;
        }

        public int GetModuleCourseId(int moduleId)
        {
            Module module = _moduleDAL.SelectById(moduleId);
            return module == null ? 0 : module.CourseID;
        }

        // ---- Admin quiz management -----------------------------------------

        public List<QuizListItem> GetAllQuizzesForAdmin()
        {
            return _quizDAL.SelectAllForAdmin();
        }

        public int CreateQuiz(int moduleId, string title, int passMarkPercent)
        {
            ValidateQuiz(title, passMarkPercent);
            if (_quizDAL.SelectByModuleId(moduleId) != null)
                throw new ValidationException("This module already has a quiz.");
            return _quizDAL.Insert(new Quiz
            {
                ModuleID = moduleId,
                QuizTitle = title,
                PassMarkPercent = passMarkPercent
            });
        }

        public void UpdateQuiz(int quizId, string title, int passMarkPercent)
        {
            ValidateQuiz(title, passMarkPercent);
            _quizDAL.Update(new Quiz
            {
                QuizID = quizId,
                // ModuleID is intentionally 0: the UPDATE statement does not
                // touch that column, so this placeholder never reaches the DB.
                ModuleID = 0,
                QuizTitle = title,
                PassMarkPercent = passMarkPercent
            });
        }

        public void DeleteQuiz(int quizId)
        {
            if (_resultDAL.GetAttemptCountByQuizId(quizId) > 0)
                throw new ValidationException("This quiz already has attempts and cannot be deleted.");

            // Questions and Answers are ON DELETE CASCADE from Quiz, so
            // deleting the quiz removes the whole question bank. The explicit
            // child deletes below are what the previous code did; they are kept
            // out now precisely because the schema guarantees the cascade.
            _quizDAL.Delete(quizId);
        }

        private void ValidateQuiz(string title, int passMarkPercent)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ValidationException("Quiz title is required.");
            if (passMarkPercent < 0 || passMarkPercent > 100)
                throw new ValidationException("Pass mark must be between 0 and 100.");
        }
    }
}
