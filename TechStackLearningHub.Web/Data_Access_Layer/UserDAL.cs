using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Data_Access_Layer
{
    public class UserDAL
    {
        /// <summary>
        /// Login/registration lookup. Includes the password columns, so this
        /// must never be used to populate a list bound to a page.
        /// </summary>
        public User SelectByUsername(string username)
        {
            const string sql =
                "SELECT UserID, Username, PasswordHash, PasswordSalt, Email, RoleID, IsActive, CreatedDate " +
                "FROM Users WHERE Username = @Username";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@Username", SqlDbType.NVarChar, 50, username);
                con.Open();
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return r.Read() ? Map(r) : null;
                }
            }
        }

        public int Insert(User user)
        {
            const string sql =
                "INSERT INTO Users (Username, PasswordHash, PasswordSalt, Email, RoleID, IsActive, CreatedDate) " +
                "VALUES (@Username, @PasswordHash, @PasswordSalt, @Email, @RoleID, @IsActive, @CreatedDate); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@Username", SqlDbType.NVarChar, 50, user.Username);
                DbHelper.AddParam(cmd, "@PasswordHash", SqlDbType.NVarChar, 256, user.PasswordHash);
                DbHelper.AddParam(cmd, "@PasswordSalt", SqlDbType.NVarChar, 256, user.PasswordSalt);
                DbHelper.AddParam(cmd, "@Email", SqlDbType.NVarChar, 100, user.Email);
                DbHelper.AddParam(cmd, "@RoleID", SqlDbType.Int, 0, user.RoleID);
                DbHelper.AddParam(cmd, "@IsActive", SqlDbType.Bit, 0, user.IsActive);
                DbHelper.AddParam(cmd, "@CreatedDate", SqlDbType.DateTime, 0, DateTime.Now);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public void UpdateStatus(int userId, bool isActive)
        {
            const string sql = "UPDATE Users SET IsActive = @IsActive WHERE UserID = @UserId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@IsActive", SqlDbType.Bit, 0, isActive);
                DbHelper.AddParam(cmd, "@UserId", SqlDbType.Int, 0, userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateRole(int userId, int roleId)
        {
            const string sql = "UPDATE Users SET RoleID = @RoleId WHERE UserID = @UserId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@RoleId", SqlDbType.Int, 0, roleId);
                DbHelper.AddParam(cmd, "@UserId", SqlDbType.Int, 0, userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public string GetRoleNameById(int roleId)
        {
            const string sql = "SELECT RoleName FROM Roles WHERE RoleID = @RoleId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@RoleId", SqlDbType.Int, 0, roleId);
                con.Open();
                object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? null : value.ToString();
            }
        }

        public int GetRoleIdByName(string roleName)
        {
            const string sql = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@RoleName", SqlDbType.NVarChar, 20, roleName);
                con.Open();
                object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0 : (int)value;
            }
        }

        /// <summary>
        /// Admin user list. Deliberately does NOT select PasswordHash or
        /// PasswordSalt - there is no reason for an admin grid to pull them
        /// out of the database at all. SelectByUsername above is the only
        /// query that reads them.
        /// </summary>
        public List<User> SelectAll()
        {
            var list = new List<User>();
            const string sql =
                "SELECT u.UserID, u.Username, u.Email, u.IsActive, u.CreatedDate, r.RoleID, r.RoleName " +
                "FROM Users u JOIN Roles r ON r.RoleID = u.RoleID ORDER BY u.Username";

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

        public int GetActiveStudentCount()
        {
            const string sql =
                "SELECT COUNT(*) FROM Users u " +
                "WHERE u.IsActive = 1 AND u.RoleID = (SELECT RoleID FROM Roles WHERE RoleName = 'Student')";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // Serves two different column lists: SelectByUsername selects the
        // password columns and no RoleName, SelectAll does the reverse. The
        // HasColumn guards let one Map cover both and return null for whatever
        // the query did not ask for, instead of duplicating the mapping.
        private User Map(IDataRecord r)
        {
            return new User
            {
                UserID = DbHelper.GetInt(r, "UserID"),
                Username = DbHelper.GetString(r, "Username"),
                PasswordHash = DbHelper.HasColumn(r, "PasswordHash") ? DbHelper.GetString(r, "PasswordHash") : null,
                PasswordSalt = DbHelper.HasColumn(r, "PasswordSalt") ? DbHelper.GetString(r, "PasswordSalt") : null,
                Email = DbHelper.GetString(r, "Email"),
                RoleID = DbHelper.GetInt(r, "RoleID"),
                RoleName = DbHelper.HasColumn(r, "RoleName") ? DbHelper.GetString(r, "RoleName") : null,
                IsActive = DbHelper.GetBool(r, "IsActive"),
                CreatedDate = DbHelper.GetDate(r, "CreatedDate")
            };
        }
    }
}
