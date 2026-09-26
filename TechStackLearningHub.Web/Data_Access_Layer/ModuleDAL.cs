using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.Models;

namespace TechStackLearningHub.Data_Access_Layer
{
    public class ModuleDAL
    {
        private const string SelectColumns = "ModuleID, CourseID, ModuleTitle, ModuleOrder";

        public List<Module> SelectByCourseId(int courseId)
        {
            var list = new List<Module>();
            const string sql = "SELECT " + SelectColumns + " FROM Modules " +
                               "WHERE CourseID = @CourseId ORDER BY ModuleOrder";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@CourseId", SqlDbType.Int, 0, courseId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) list.Add(Map(r));
                }
            }
            return list;
        }

        public Module SelectById(int moduleId)
        {
            const string sql = "SELECT " + SelectColumns + " FROM Modules WHERE ModuleID = @ModuleId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@ModuleId", SqlDbType.Int, 0, moduleId);
                con.Open();
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return r.Read() ? Map(r) : null;
                }
            }
        }

        public int Insert(Module module)
        {
            const string sql =
                "INSERT INTO Modules (CourseID, ModuleTitle, ModuleOrder) " +
                "VALUES (@CourseID, @ModuleTitle, @ModuleOrder); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@CourseID", SqlDbType.Int, 0, module.CourseID);
                DbHelper.AddParam(cmd, "@ModuleTitle", SqlDbType.NVarChar, 150, module.ModuleTitle);
                DbHelper.AddParam(cmd, "@ModuleOrder", SqlDbType.Int, 0, module.ModuleOrder);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public void Update(Module module)
        {
            const string sql =
                "UPDATE Modules SET ModuleTitle = @ModuleTitle, ModuleOrder = @ModuleOrder " +
                "WHERE ModuleID = @ModuleId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@ModuleTitle", SqlDbType.NVarChar, 150, module.ModuleTitle);
                DbHelper.AddParam(cmd, "@ModuleOrder", SqlDbType.Int, 0, module.ModuleOrder);
                DbHelper.AddParam(cmd, "@ModuleId", SqlDbType.Int, 0, module.ModuleID);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int moduleId)
        {
            const string sql = "DELETE FROM Modules WHERE ModuleID = @ModuleId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@ModuleId", SqlDbType.Int, 0, moduleId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Reorder(int moduleId, int newOrder)
        {
            const string sql = "UPDATE Modules SET ModuleOrder = @NewOrder WHERE ModuleID = @ModuleId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@NewOrder", SqlDbType.Int, 0, newOrder);
                DbHelper.AddParam(cmd, "@ModuleId", SqlDbType.Int, 0, moduleId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public int GetCountByCourseId(int courseId)
        {
            const string sql = "SELECT COUNT(*) FROM Modules WHERE CourseID = @CourseId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@CourseId", SqlDbType.Int, 0, courseId);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        private Module Map(IDataRecord r)
        {
            return new Module
            {
                ModuleID = DbHelper.GetInt(r, "ModuleID"),
                CourseID = DbHelper.GetInt(r, "CourseID"),
                ModuleTitle = DbHelper.GetString(r, "ModuleTitle"),
                ModuleOrder = DbHelper.GetInt(r, "ModuleOrder")
            };
        }
    }
}
