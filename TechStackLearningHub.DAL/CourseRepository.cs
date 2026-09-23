using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.DAL
{
    public class CourseRepository
    {
        public DataTable GetPublishedCourses()
        {
            const string sql = "SELECT * FROM Courses WHERE IsPublished = 1 ORDER BY TechStack, CourseName";
            return DbHelper.ExecuteQuery(sql, null);
        }

        public DataTable GetAllCoursesForAdmin()
        {
            const string sql = "SELECT * FROM Courses ORDER BY CreatedDate DESC";
            return DbHelper.ExecuteQuery(sql, null);
        }

        public Course GetCourseById(int courseId)
        {
            const string sql = "SELECT * FROM Courses WHERE CourseID = @CourseId";
            DataTable table = DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@CourseId", courseId) });
            if (table.Rows.Count == 0) return null;
            return MapRow(table.Rows[0]);
        }

        public int InsertCourse(Course course)
        {
            const string sql = "INSERT INTO Courses (CourseName, TechStack, Description, IsPublished, CreatedDate) " +
                               "VALUES (@CourseName, @TechStack, @Description, @IsPublished, @CreatedDate); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            SqlParameter[] parameters =
            {
                new SqlParameter("@CourseName", course.CourseName),
                new SqlParameter("@TechStack", course.TechStack),
                new SqlParameter("@Description", (object)course.Description ?? System.DBNull.Value),
                new SqlParameter("@IsPublished", course.IsPublished),
                new SqlParameter("@CreatedDate", System.DateTime.Now)
            };
            return (int)DbHelper.ExecuteScalar(sql, parameters);
        }

        public void UpdateCourse(Course course)
        {
            const string sql = "UPDATE Courses SET CourseName = @CourseName, TechStack = @TechStack, " +
                               "Description = @Description WHERE CourseID = @CourseId";
            SqlParameter[] parameters =
            {
                new SqlParameter("@CourseName", course.CourseName),
                new SqlParameter("@TechStack", course.TechStack),
                new SqlParameter("@Description", (object)course.Description ?? System.DBNull.Value),
                new SqlParameter("@CourseId", course.CourseID)
            };
            DbHelper.ExecuteNonQuery(sql, parameters);
        }

        public void SetPublishStatus(int courseId, bool isPublished)
        {
            const string sql = "UPDATE Courses SET IsPublished = @IsPublished WHERE CourseID = @CourseId";
            SqlParameter[] parameters =
            {
                new SqlParameter("@IsPublished", isPublished),
                new SqlParameter("@CourseId", courseId)
            };
            DbHelper.ExecuteNonQuery(sql, parameters);
        }

        public void DeleteCourse(int courseId)
        {
            const string sql = "DELETE FROM Courses WHERE CourseID = @CourseId";
            DbHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@CourseId", courseId) });
        }

        public int GetPublishedCourseCount()
        {
            const string sql = "SELECT COUNT(*) FROM Courses WHERE IsPublished = 1";
            return (int)DbHelper.ExecuteScalar(sql, null);
        }

        private Course MapRow(DataRow row)
        {
            return new Course
            {
                CourseID = (int)row["CourseID"],
                CourseName = row["CourseName"].ToString(),
                TechStack = row["TechStack"].ToString(),
                Description = row["Description"] == System.DBNull.Value ? null : row["Description"].ToString(),
                IsPublished = (bool)row["IsPublished"],
                CreatedDate = (System.DateTime)row["CreatedDate"]
            };
        }
    }
}