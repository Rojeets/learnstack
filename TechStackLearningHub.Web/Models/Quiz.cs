namespace TechStackLearningHub.Web.Models
{
    public class Quiz
    {
        public int QuizID { get; set; }
        public int ModuleID { get; set; }
        public string QuizTitle { get; set; }
        public int PassMarkPercent { get; set; }

        /// <summary>
        /// Time allowed for the attempt. NULL means the quiz has never had a
        /// limit configured, and readers fall back to
        /// <see cref="QuizBLL.DefaultDurationMinutes"/> rather than treating it
        /// as zero, so a NULL row can never mean "no time at all".
        /// </summary>
        public int? DurationMinutes { get; set; }
    }
}