using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.DAL
{
    public class UserRepository
    {
        public User GetUserByUsername(string username)
        {
            const string sql = "SELECT UserID, Username, PasswordHash, PasswordSalt, Email, RoleID, IsActive, CreatedDate " +
                               "FROM Users WHERE Username = @Username";
            DataTable table = DbHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@Username", username) });
            if (table.Rows.Count == 0) return null;
            return MapRow(table.Rows[0]);
        }

        public int InsertUser(User user)
        {
            const string sql = "INSERT INTO Users (Username, PasswordHash, PasswordSalt, Email, RoleID, IsActive, CreatedDate) " +
                               "VALUES (@Username, @PasswordHash, @PasswordSalt, @Email, @RoleID, @IsActive, @CreatedDate); " +
                               "SELECT CAST(SCOPE_IDENTITY() AS INT);";
            SqlParameter[] parameters =
            {
                new SqlParameter("@Username", user.Username),
                new SqlParameter("@PasswordHash", user.PasswordHash),
                new SqlParameter("@PasswordSalt", user.PasswordSalt),
                new SqlParameter("@Email", user.Email),
                new SqlParameter("@RoleID", user.RoleID),
                new SqlParameter("@IsActive", user.IsActive),
                new SqlParameter("@CreatedDate", System.DateTime.Now)
            };
            return (int)DbHelper.ExecuteScalar(sql, parameters);
        }

        public void UpdateUserStatus(int userId, bool isActive)
        {
            const string sql = "UPDATE Users SET IsActive = @IsActive WHERE UserID = @UserId";
            SqlParameter[] parameters =
            {
                new SqlParameter("@IsActive", isActive),
                new SqlParameter("@UserId", userId)
            };
            DbHelper.ExecuteNonQuery(sql, parameters);
        }

        public void UpdateUserRole(int userId, int roleId)
        {
            const string sql = "UPDATE Users SET RoleID = @RoleId WHERE UserID = @UserId";
            SqlParameter[] parameters =
            {
                new SqlParameter("@RoleId", roleId),
                new SqlParameter("@UserId", userId)
            };
            DbHelper.ExecuteNonQuery(sql, parameters);
        }

        public string GetRoleNameById(int roleId)
        {
            const string sql = "SELECT RoleName FROM Roles WHERE RoleID = @RoleId";
            object value = DbHelper.ExecuteScalar(sql, new SqlParameter[] { new SqlParameter("@RoleId", roleId) });
            return value == null ? null : value.ToString();
        }

        public int GetRoleIdByName(string roleName)
        {
            const string sql = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
            object value = DbHelper.ExecuteScalar(sql, new SqlParameter[] { new SqlParameter("@RoleName", roleName) });
            return value == null ? 0 : (int)value;
        }

        public DataTable GetAllUsers()
        {
            const string sql = "SELECT u.UserID, u.Username, u.Email, u.IsActive, u.CreatedDate, r.RoleID, r.RoleName " +
                               "FROM Users u JOIN Roles r ON r.RoleID = u.RoleID ORDER BY u.Username";
            return DbHelper.ExecuteQuery(sql, null);
        }

        public int GetActiveStudentCount()
        {
            const string sql = "SELECT COUNT(*) FROM Users u " +
                               "WHERE u.IsActive = 1 AND u.RoleID = (SELECT RoleID FROM Roles WHERE RoleName = 'Student')";
            return (int)DbHelper.ExecuteScalar(sql, null);
        }

        private User MapRow(DataRow row)
        {
            return new User
            {
                UserID = (int)row["UserID"],
                Username = row["Username"].ToString(),
                PasswordHash = row["PasswordHash"].ToString(),
                PasswordSalt = row["PasswordSalt"].ToString(),
                Email = row["Email"].ToString(),
                RoleID = (int)row["RoleID"],
                IsActive = (bool)row["IsActive"],
                CreatedDate = (System.DateTime)row["CreatedDate"]
            };
        }
    }
}