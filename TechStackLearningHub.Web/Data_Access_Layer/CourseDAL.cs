using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Data_Access_Layer
{
    /// <summary>
    /// All SQL for the Courses table. Every method follows the same shape:
    /// open connection, build command, add parameters, execute, map results.
    /// </summary>
    public class CourseDAL
    {
        private const string SelectColumns =
            "CourseID, CourseName, TechStack, Description, IsPublished, CreatedDate";

        public List<Course> SelectPublishedCourses()
        {
            var list = new List<Course>();
            const string sql = "SELECT " + SelectColumns + " FROM Courses " +
                               "WHERE IsPublished = 1 ORDER BY TechStack, CourseName";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) list.Add(Map(r));
                }
            }
            return list;
        }

        public List<Course> SelectAllForAdmin()
        {
            var list = new List<Course>();
            const string sql = "SELECT " + SelectColumns + " FROM Courses ORDER BY CreatedDate DESC";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) list.Add(Map(r));
                }
            }
            return list;
        }

        public Course SelectById(int courseId)
        {
            const string sql = "SELECT " + SelectColumns + " FROM Courses WHERE CourseID = @CourseId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@CourseId", SqlDbType.Int, 0, courseId);
                con.Open();
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return r.Read() ? Map(r) : null;
                }
            }
        }

        public int Insert(Course course)
        {
            const string sql =
                "INSERT INTO Courses (CourseName, TechStack, Description, IsPublished, CreatedDate) " +
                "VALUES (@CourseName, @TechStack, @Description, @IsPublished, @CreatedDate); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@CourseName", SqlDbType.NVarChar, 150, course.CourseName);
                DbHelper.AddParam(cmd, "@TechStack", SqlDbType.NVarChar, 50, course.TechStack);
                DbHelper.AddParam(cmd, "@Description", SqlDbType.NVarChar, 1000, course.Description);
                DbHelper.AddParam(cmd, "@IsPublished", SqlDbType.Bit, 0, course.IsPublished);
                DbHelper.AddParam(cmd, "@CreatedDate", SqlDbType.DateTime, 0, DateTime.Now);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // IsPublished is deliberately NOT in this statement. Publishing goes
        // through SetPublishStatus, which first asserts the course actually
        // has content. Letting a general edit flip the flag would bypass that
        // rule.
        public void Update(Course course)
        {
            const string sql =
                "UPDATE Courses SET CourseName = @CourseName, TechStack = @TechStack, " +
                "Description = @Description WHERE CourseID = @CourseId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@CourseName", SqlDbType.NVarChar, 150, course.CourseName);
                DbHelper.AddParam(cmd, "@TechStack", SqlDbType.NVarChar, 50, course.TechStack);
                DbHelper.AddParam(cmd, "@Description", SqlDbType.NVarChar, 1000, course.Description);
                DbHelper.AddParam(cmd, "@CourseId", SqlDbType.Int, 0, course.CourseID);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void SetPublishStatus(int courseId, bool isPublished)
        {
            const string sql = "UPDATE Courses SET IsPublished = @IsPublished WHERE CourseID = @CourseId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@IsPublished", SqlDbType.Bit, 0, isPublished);
                DbHelper.AddParam(cmd, "@CourseId", SqlDbType.Int, 0, courseId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int courseId)
        {
            const string sql = "DELETE FROM Courses WHERE CourseID = @CourseId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@CourseId", SqlDbType.Int, 0, courseId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public int GetPublishedCourseCount()
        {
            const string sql = "SELECT COUNT(*) FROM Courses WHERE IsPublished = 1";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // All four read methods above select the same column list, so one Map
        // covers them. ModuleCount is a computed property, never selected -
        // CourseBLL fills it in.
        private Course Map(IDataRecord r)
        {
            return new Course
            {
                CourseID = DbHelper.GetInt(r, "CourseID"),
                CourseName = DbHelper.GetString(r, "CourseName"),
                TechStack = DbHelper.GetString(r, "TechStack"),
                Description = DbHelper.GetString(r, "Description"),
                IsPublished = DbHelper.GetBool(r, "IsPublished"),
                CreatedDate = DbHelper.GetDate(r, "CreatedDate")
            };
        }
    }
}
