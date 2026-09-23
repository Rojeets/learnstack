using System.Data;
using TechStackLearningHub.DAL;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.BLL
{
    public class CourseService
    {
        // The fixed TechStack labels surfaced in the admin dropdown and used to
        // validate course creation. Matches the catalogue filter options.
        public static readonly string[] KnownTechStacks =
        {
            "ASP.NET", "React", "Python", "Java", "Node.js",
            "PHP/Laravel", "Android (Kotlin)", "Flutter"
        };

        private readonly CourseRepository _courseRepository = new CourseRepository();
        private readonly ModuleRepository _moduleRepository = new ModuleRepository();
        private readonly ModuleService _moduleService = new ModuleService();

        public DataTable GetCatalogueForStudents(string techStackFilter = null)
        {
            DataTable courses = _courseRepository.GetPublishedCourses();
            courses.Columns.Add("ModuleCount", typeof(int));
            foreach (DataRow row in courses.Rows)
            {
                row["ModuleCount"] = _moduleRepository.GetModuleCountByCourseId((int)row["CourseID"]);
            }
            if (!string.IsNullOrEmpty(techStackFilter))
            {
                DataRow[] filtered = courses.Select("TechStack = '" + techStackFilter.Replace("'", "''") + "'");
                DataTable result = courses.Clone();
                foreach (DataRow row in filtered)
                    result.ImportRow(row);
                return result;
            }
            return courses;
        }

        public Course GetCourseForAdminEdit(int courseId)
        {
            return _courseRepository.GetCourseById(courseId);
        }

        // Student-facing lookup: null unless the course is published, so a
        // hand-typed URL can never open a draft course.
        public Course GetPublishedCourse(int courseId)
        {
            Course course = _courseRepository.GetCourseById(courseId);
            if (course == null || !course.IsPublished)
                return null;
            return course;
        }

        public int CreateCourse(Course course)
        {
            ValidateCourse(course);
            return _courseRepository.InsertCourse(course);
        }

        public void UpdateCourse(Course course)
        {
            ValidateCourse(course);
            _courseRepository.UpdateCourse(course);
        }

        public void PublishCourse(int courseId)
        {
            if (!_moduleService.CourseHasContent(courseId))
                throw new System.InvalidOperationException(
                    "This course has no content yet. Add at least one module with one lesson before publishing.");
            _courseRepository.SetPublishStatus(courseId, true);
        }

        public void UnpublishCourse(int courseId)
        {
            _courseRepository.SetPublishStatus(courseId, false);
        }

        public DataTable GetAllCoursesForAdmin()
        {
            return _courseRepository.GetAllCoursesForAdmin();
        }

        public void DeleteCourse(int courseId)
        {
            _courseRepository.DeleteCourse(courseId);
        }

        public int GetPublishedCourseCount()
        {
            return _courseRepository.GetPublishedCourseCount();
        }

        private void ValidateCourse(Course course)
        {
            if (string.IsNullOrWhiteSpace(course.CourseName))
                throw new System.InvalidOperationException("Course name is required.");
            bool known = false;
            foreach (string stack in KnownTechStacks)
            {
                if (string.Equals(stack, course.TechStack, System.StringComparison.OrdinalIgnoreCase))
                {
                    known = true;
                    break;
                }
            }
            if (!known)
                throw new System.InvalidOperationException("TechStack must be one of the known labels.");
        }
    }
}