namespace TechStackLearningHub.Web.Models
{
    /// <summary>
    /// One row of the admin dashboard's "courses by tech stack" bars.
    /// TechStack and CourseCount come straight from a SQL GROUP BY;
    /// PercentOfMax cannot, because it depends on every other row, so
    /// CourseBLL fills it in as a relative-to-the-largest-stack figure.
    /// </summary>
    public class TechStackCourseCount
    {
        public string TechStack { get; set; }
        public int CourseCount { get; set; }
        public decimal PercentOfMax { get; set; }
    }
}
