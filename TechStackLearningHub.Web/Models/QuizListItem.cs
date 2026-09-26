namespace TechStackLearningHub.Web.Models
{
    /// <summary>
    /// One row for the admin quiz picker, which spans every course and module
    /// so the admin can choose any quiz to edit. A flat join result rather
    /// than a Quiz, because the picker shows ModuleTitle and CourseName and
    /// the Quiz entity stores neither.
    /// </summary>
    public class QuizListItem
    {
        public int QuizID { get; set; }
        public string QuizTitle { get; set; }
        public int PassMarkPercent { get; set; }
        public int? DurationMinutes { get; set; }
        public string ModuleTitle { get; set; }
        public string CourseName { get; set; }
    }
}
