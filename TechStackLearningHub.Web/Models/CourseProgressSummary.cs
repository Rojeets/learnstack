namespace TechStackLearningHub.Models
{
    /// <summary>
    /// A student's completion percentage for one course, as shown on the
    /// student dashboard and the progress dashboard. Computed in SQL from
    /// Lessons LEFT JOIN Progress, so courses the student has not touched
    /// produce no row rather than a 0% row.
    /// </summary>
    public class CourseProgressSummary
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string TechStack { get; set; }
        public decimal PercentComplete { get; set; }
    }
}
