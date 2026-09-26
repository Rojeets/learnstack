using System;

namespace TechStackLearningHub.Models
{
    public class Result
    {
        public int ResultID { get; set; }
        public int UserID { get; set; }
        public int QuizID { get; set; }
        public decimal Score { get; set; }
        public DateTime AttemptDate { get; set; }
        public bool IsPassed { get; set; }
    }
}