using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.DAL
{
    public class ModuleRepository
    {
        public DataTable GetModulesByCourseId(int courseId)
        {
            const string sql = "SELECT * FROM Modules WHERE CourseID = @CourseId ORDER BY ModuleOrder";
            return DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@CourseId", courseId) });
        }

        public Module GetModuleById(int moduleId)
        {
            const string sql = "SELECT * FROM Modules WHERE ModuleID = @ModuleId";
            DataTable table = DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@ModuleId", moduleId) });
            if (table.Rows.Count == 0) return null;
            return MapRow(table.Rows[0]);
        }

        public int InsertModule(Module module)
        {
            const string sql = "INSERT INTO Modules (CourseID, ModuleTitle, ModuleOrder) " +
                               "VALUES (@CourseID, @ModuleTitle, @ModuleOrder); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            SqlParameter[] parameters =
            {
                new SqlParameter("@CourseID", module.CourseID),
                new SqlParameter("@ModuleTitle", module.ModuleTitle),
                new SqlParameter("@ModuleOrder", module.ModuleOrder)
            };
            return (int)DbHelper.ExecuteScalar(sql, parameters);
        }

        public void UpdateModule(Module module)
        {
            const string sql = "UPDATE Modules SET ModuleTitle = @ModuleTitle, ModuleOrder = @ModuleOrder " +
                               "WHERE ModuleID = @ModuleId";
            SqlParameter[] parameters =
            {
                new SqlParameter("@ModuleTitle", module.ModuleTitle),
                new SqlParameter("@ModuleOrder", module.ModuleOrder),
                new SqlParameter("@ModuleId", module.ModuleID)
            };
            DbHelper.ExecuteNonQuery(sql, parameters);
        }

        public void DeleteModule(int moduleId)
        {
            const string sql = "DELETE FROM Modules WHERE ModuleID = @ModuleId";
            DbHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@ModuleId", moduleId) });
        }

        public void ReorderModule(int moduleId, int newOrder)
        {
            const string sql = "UPDATE Modules SET ModuleOrder = @NewOrder WHERE ModuleID = @ModuleId";
            SqlParameter[] parameters =
            {
                new SqlParameter("@NewOrder", newOrder),
                new SqlParameter("@ModuleId", moduleId)
            };
            DbHelper.ExecuteNonQuery(sql, parameters);
        }

        public int GetModuleCountByCourseId(int courseId)
        {
            const string sql = "SELECT COUNT(*) FROM Modules WHERE CourseID = @CourseId";
            return (int)DbHelper.ExecuteScalar(sql, new SqlParameter[] { new SqlParameter("@CourseId", courseId) });
        }

        private Module MapRow(DataRow row)
        {
            return new Module
            {
                ModuleID = (int)row["ModuleID"],
                CourseID = (int)row["CourseID"],
                ModuleTitle = row["ModuleTitle"].ToString(),
                ModuleOrder = (int)row["ModuleOrder"]
            };
        }
    }
}