using System;
using System.Collections.Generic;
using TechStackLearningHub.Web.Data_Access_Layer;
using TechStackLearningHub.Web.Helpers;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.BLL
{
    public class CourseBLL
    {
        // The fixed TechStack labels surfaced in the admin dropdown and used to
        // validate course creation. Matches the catalogue filter options.
        public static readonly string[] KnownTechStacks =
        {
            "ASP.NET", "React", "Python", "Java", "Node.js",
            "PHP/Laravel", "Android (Kotlin)", "Flutter"
        };

        private readonly CourseDAL _courseDAL = new CourseDAL();
        private readonly ModuleDAL _moduleDAL = new ModuleDAL();
        private readonly ModuleBLL _moduleBLL = new ModuleBLL();

        public List<Course> GetCatalogueForStudents(string techStackFilter = null)
        {
            List<Course> courses = _courseDAL.SelectPublishedCourses();

            // ModuleCount is a computed property, not a column, so the DAL
            // cannot select it. Filling it in here keeps that concern out of
            // the SQL and means one query per course is acceptable - the
            // catalogue is small and admin-authored.
            foreach (Course course in courses)
            {
                course.ModuleCount = _moduleDAL.GetCountByCourseId(course.CourseID);
            }

            if (string.IsNullOrEmpty(techStackFilter))
                return courses;

            // Filtered with an ordinal comparison on the typed list. The
            // earlier DataTable version had to avoid DataTable.Select's filter
            // string precisely because that value would have been parsed as
            // filter-expression syntax; with a List<Course> the value can only
            // ever be compared, never interpreted.
            var filtered = new List<Course>();
            foreach (Course course in courses)
            {
                if (string.Equals(course.TechStack, techStackFilter,
                        StringComparison.OrdinalIgnoreCase))
                {
                    filtered.Add(course);
                }
            }
            return filtered;
        }

        public Course GetCourseForAdminEdit(int courseId)
        {
            return _courseDAL.SelectById(courseId);
        }

        // Student-facing lookup: null unless the course is published, so a
        // hand-typed URL can never open a draft course.
        public Course GetPublishedCourse(int courseId)
        {
            Course course = _courseDAL.SelectById(courseId);
            if (course == null || !course.IsPublished)
                return null;
            return course;
        }

        public int CreateCourse(Course course)
        {
            ValidateCourse(course);
            return _courseDAL.Insert(course);
        }

        public void UpdateCourse(Course course)
        {
            ValidateCourse(course);
            _courseDAL.Update(course);
        }

        public void PublishCourse(int courseId)
        {
            if (!_moduleBLL.CourseHasContent(courseId))
                throw new ValidationException(
                    "This course has no content yet. Add at least one module with one lesson before publishing.");
            _courseDAL.SetPublishStatus(courseId, true);
        }

        public void UnpublishCourse(int courseId)
        {
            _courseDAL.SetPublishStatus(courseId, false);
        }

        public List<Course> GetAllCoursesForAdmin()
        {
            return _courseDAL.SelectAllForAdmin();
        }

        public void DeleteCourse(int courseId)
        {
            _courseDAL.Delete(courseId);
        }

        public int GetPublishedCourseCount()
        {
            return _courseDAL.GetPublishedCourseCount();
        }

        /// <summary>
        /// Courses per tech stack for the admin dashboard bars, with
        /// PercentOfMax filled in.
        /// </summary>
        public List<TechStackCourseCount> GetCourseCountsByTechStack()
        {
            List<TechStackCourseCount> counts = _courseDAL.SelectCourseCountsByTechStack();
            if (counts.Count == 0)
                return counts;

            // The bar width is relative to the biggest stack, not to the total.
            // Dividing by the total would give the widest bar a value near 100
            // only when one stack holds nearly everything; in a realistic
            // spread every bar would shrink to a few percent and the chart
            // would lose the comparison it exists to make. Normalising to the
            // max also means the widest bar is always full width, whatever the
            // absolute numbers are.
            int max = 0;
            foreach (TechStackCourseCount row in counts)
            {
                if (row.CourseCount > max)
                    max = row.CourseCount;
            }

            if (max == 0)
                return counts;

            foreach (TechStackCourseCount row in counts)
            {
                row.PercentOfMax = (decimal)row.CourseCount * 100m / max;
            }
            return counts;
        }

        private void ValidateCourse(Course course)
        {
            if (course == null)
                throw new ValidationException("Course not supplied.");

            if (string.IsNullOrWhiteSpace(course.CourseName))
                throw new ValidationException("Course name is required.");

            bool known = false;
            foreach (string stack in KnownTechStacks)
            {
                if (string.Equals(stack, course.TechStack, StringComparison.OrdinalIgnoreCase))
                {
                    known = true;
                    break;
                }
            }
            if (!known)
                throw new ValidationException("TechStack must be one of the known labels.");
        }
    }
}
