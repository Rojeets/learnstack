using System;

namespace TechStackLearningHub.Web.Models
{
    /// <summary>
    /// One row in a student's quiz history. Joined out of Results + Quiz +
    /// Modules, so it is a read model rather than an entity.
    /// </summary>
    public class AttemptSummary
    {
        public string QuizTitle { get; set; }
        public decimal Score { get; set; }
        public DateTime AttemptDate { get; set; }
        public bool IsPassed { get; set; }
        public string ModuleTitle { get; set; }
    }
}
