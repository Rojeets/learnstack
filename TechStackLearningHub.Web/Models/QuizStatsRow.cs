namespace TechStackLearningHub.Models
{
    /// <summary>
    /// Aggregated attempt statistics for a single quiz, used by the admin
    /// reports screen. Every value is a SQL aggregate, so none of it is
    /// stored on any table.
    /// </summary>
    public class QuizStatsRow
    {
        public string QuizTitle { get; set; }
        public int AttemptCount { get; set; }
        public decimal AverageScore { get; set; }
        public decimal PassRate { get; set; }
    }
}
