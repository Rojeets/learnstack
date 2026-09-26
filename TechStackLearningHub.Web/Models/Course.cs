using System;

namespace TechStackLearningHub.Models
{
    public class Course
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string TechStack { get; set; }
        public string Description { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModuleCount { get; set; }
    }
}