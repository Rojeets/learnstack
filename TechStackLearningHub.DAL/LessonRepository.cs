using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.DAL
{
    public class LessonRepository
    {
        public DataTable GetLessonsByModuleId(int moduleId)
        {
            const string sql = "SELECT * FROM Lessons WHERE ModuleID = @ModuleId ORDER BY LessonOrder";
            return DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@ModuleId", moduleId) });
        }

        public Lesson GetLessonById(int lessonId)
        {
            const string sql = "SELECT * FROM Lessons WHERE LessonID = @LessonId";
            DataTable table = DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@LessonId", lessonId) });
            if (table.Rows.Count == 0) return null;
            return MapRow(table.Rows[0]);
        }

        public int InsertLesson(Lesson lesson)
        {
            const string sql = "INSERT INTO Lessons (ModuleID, LessonTitle, ContentHTML, VideoUrl, NotesFilePath, LessonOrder) " +
                               "VALUES (@ModuleID, @LessonTitle, @ContentHTML, @VideoUrl, @NotesFilePath, @LessonOrder); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            SqlParameter[] parameters =
            {
                new SqlParameter("@ModuleID", lesson.ModuleID),
                new SqlParameter("@LessonTitle", lesson.LessonTitle),
                new SqlParameter("@ContentHTML", (object)lesson.ContentHTML ?? System.DBNull.Value),
                new SqlParameter("@VideoUrl", (object)lesson.VideoUrl ?? System.DBNull.Value),
                new SqlParameter("@NotesFilePath", (object)lesson.NotesFilePath ?? System.DBNull.Value),
                new SqlParameter("@LessonOrder", lesson.LessonOrder)
            };
            return (int)DbHelper.ExecuteScalar(sql, parameters);
        }

        public void UpdateLesson(Lesson lesson)
        {
            const string sql = "UPDATE Lessons SET LessonTitle = @LessonTitle, ContentHTML = @ContentHTML, " +
                               "VideoUrl = @VideoUrl, NotesFilePath = @NotesFilePath, LessonOrder = @LessonOrder " +
                               "WHERE LessonID = @LessonId";
            SqlParameter[] parameters =
            {
                new SqlParameter("@LessonTitle", lesson.LessonTitle),
                new SqlParameter("@ContentHTML", (object)lesson.ContentHTML ?? System.DBNull.Value),
                new SqlParameter("@VideoUrl", (object)lesson.VideoUrl ?? System.DBNull.Value),
                new SqlParameter("@NotesFilePath", (object)lesson.NotesFilePath ?? System.DBNull.Value),
                new SqlParameter("@LessonOrder", lesson.LessonOrder),
                new SqlParameter("@LessonId", lesson.LessonID)
            };
            DbHelper.ExecuteNonQuery(sql, parameters);
        }

        public void DeleteLesson(int lessonId)
        {
            const string sql = "DELETE FROM Lessons WHERE LessonID = @LessonId";
            DbHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@LessonId", lessonId) });
        }

        private Lesson MapRow(DataRow row)
        {
            return new Lesson
            {
                LessonID = (int)row["LessonID"],
                ModuleID = (int)row["ModuleID"],
                LessonTitle = row["LessonTitle"].ToString(),
                ContentHTML = row["ContentHTML"] == System.DBNull.Value ? null : row["ContentHTML"].ToString(),
                VideoUrl = row["VideoUrl"] == System.DBNull.Value ? null : row["VideoUrl"].ToString(),
                NotesFilePath = row["NotesFilePath"] == System.DBNull.Value ? null : row["NotesFilePath"].ToString(),
                LessonOrder = (int)row["LessonOrder"]
            };
        }
    }
}