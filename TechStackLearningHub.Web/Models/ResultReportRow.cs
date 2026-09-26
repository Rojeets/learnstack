using System;

namespace TechStackLearningHub.Web.Models
{
    /// <summary>
    /// One row of the admin activity report. Joins Results all the way out to
    /// Users and Courses, so it exists only as a read model. The unfiltered
    /// and course-filtered report queries return the same shape, which is why
    /// there is one type and not two.
    /// </summary>
    public class ResultReportRow
    {
        public int ResultID { get; set; }
        public string Username { get; set; }
        public string CourseName { get; set; }
        public string TechStack { get; set; }
        public string ModuleTitle { get; set; }
        public string QuizTitle { get; set; }
        public decimal Score { get; set; }
        public DateTime AttemptDate { get; set; }
        public bool IsPassed { get; set; }
    }
}
