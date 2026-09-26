namespace TechStackLearningHub.Web.Models
{
    public class Quiz
    {
        public int QuizID { get; set; }
        public int ModuleID { get; set; }
        public string QuizTitle { get; set; }
        public int PassMarkPercent { get; set; }
    }
}