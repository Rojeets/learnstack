namespace TechStackLearningHub.Web.Models
{
    /// <summary>
    /// One row of the admin progress report: every student's completion
    /// percentage for one course. Built with a CROSS JOIN from Users so
    /// students who have not started the course still appear at 0%, which is
    /// what makes the report complete rather than only showing participants.
    /// </summary>
    public class ProgressReportRow
    {
        public string Username { get; set; }
        public string CourseName { get; set; }
        public string TechStack { get; set; }
        public decimal PercentComplete { get; set; }
    }
}
