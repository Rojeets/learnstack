using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.Models;

namespace TechStackLearningHub.Data_Access_Layer
{
    public class LessonDAL
    {
        private const string SelectColumns =
            "LessonID, ModuleID, LessonTitle, ContentHTML, VideoUrl, NotesFilePath, LessonOrder";

        public List<Lesson> SelectByModuleId(int moduleId)
        {
            var list = new List<Lesson>();
            const string sql = "SELECT " + SelectColumns + " FROM Lessons " +
                               "WHERE ModuleID = @ModuleId ORDER BY LessonOrder";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@ModuleId", SqlDbType.Int, 0, moduleId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) list.Add(Map(r));
                }
            }
            return list;
        }

        public Lesson SelectById(int lessonId)
        {
            const string sql = "SELECT " + SelectColumns + " FROM Lessons WHERE LessonID = @LessonId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@LessonId", SqlDbType.Int, 0, lessonId);
                con.Open();
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return r.Read() ? Map(r) : null;
                }
            }
        }

        public int Insert(Lesson lesson)
        {
            const string sql =
                "INSERT INTO Lessons (ModuleID, LessonTitle, ContentHTML, VideoUrl, NotesFilePath, LessonOrder) " +
                "VALUES (@ModuleID, @LessonTitle, @ContentHTML, @VideoUrl, @NotesFilePath, @LessonOrder); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@ModuleID", SqlDbType.Int, 0, lesson.ModuleID);
                DbHelper.AddParam(cmd, "@LessonTitle", SqlDbType.NVarChar, 150, lesson.LessonTitle);
                // NVARCHAR(MAX) has no declared length, so size 0 is correct
                // for both ContentHTML and a plain unbounded parameter.
                DbHelper.AddParam(cmd, "@ContentHTML", SqlDbType.NVarChar, -1, lesson.ContentHTML);
                DbHelper.AddParam(cmd, "@VideoUrl", SqlDbType.NVarChar, 300, lesson.VideoUrl);
                DbHelper.AddParam(cmd, "@NotesFilePath", SqlDbType.NVarChar, 300, lesson.NotesFilePath);
                DbHelper.AddParam(cmd, "@LessonOrder", SqlDbType.Int, 0, lesson.LessonOrder);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public void Update(Lesson lesson)
        {
            const string sql =
                "UPDATE Lessons SET LessonTitle = @LessonTitle, ContentHTML = @ContentHTML, " +
                "VideoUrl = @VideoUrl, NotesFilePath = @NotesFilePath, LessonOrder = @LessonOrder " +
                "WHERE LessonID = @LessonId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@LessonTitle", SqlDbType.NVarChar, 150, lesson.LessonTitle);
                DbHelper.AddParam(cmd, "@ContentHTML", SqlDbType.NVarChar, -1, lesson.ContentHTML);
                DbHelper.AddParam(cmd, "@VideoUrl", SqlDbType.NVarChar, 300, lesson.VideoUrl);
                DbHelper.AddParam(cmd, "@NotesFilePath", SqlDbType.NVarChar, 300, lesson.NotesFilePath);
                DbHelper.AddParam(cmd, "@LessonOrder", SqlDbType.Int, 0, lesson.LessonOrder);
                DbHelper.AddParam(cmd, "@LessonId", SqlDbType.Int, 0, lesson.LessonID);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int lessonId)
        {
            const string sql = "DELETE FROM Lessons WHERE LessonID = @LessonId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@LessonId", SqlDbType.Int, 0, lessonId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private Lesson Map(IDataRecord r)
        {
            return new Lesson
            {
                LessonID = DbHelper.GetInt(r, "LessonID"),
                ModuleID = DbHelper.GetInt(r, "ModuleID"),
                LessonTitle = DbHelper.GetString(r, "LessonTitle"),
                ContentHTML = DbHelper.GetString(r, "ContentHTML"),
                VideoUrl = DbHelper.GetString(r, "VideoUrl"),
                NotesFilePath = DbHelper.GetString(r, "NotesFilePath"),
                LessonOrder = DbHelper.GetInt(r, "LessonOrder")
            };
        }
    }
}
