namespace TechStackLearningHub.Models
{
    public class Question
    {
        public int QuestionID { get; set; }
        public int QuizID { get; set; }
        public string QuestionText { get; set; }
        public int Marks { get; set; }
        public bool AllowMultiple { get; set; }
    }
}