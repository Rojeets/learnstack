using System;

namespace TechStackLearningHub.Web.Models
{
    public class Progress
    {
        public int ProgressID { get; set; }
        public int UserID { get; set; }
        public int LessonID { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletionDate { get; set; }
    }
}