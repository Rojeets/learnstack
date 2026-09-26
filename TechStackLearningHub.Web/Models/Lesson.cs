namespace TechStackLearningHub.Models
{
    public class Lesson
    {
        public int LessonID { get; set; }
        public int ModuleID { get; set; }
        public string LessonTitle { get; set; }
        public string ContentHTML { get; set; }
        public string VideoUrl { get; set; }
        public string NotesFilePath { get; set; }
        public int LessonOrder { get; set; }
    }
}