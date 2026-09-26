using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Data_Access_Layer
{
    /// <summary>
    /// What SelectResetTokenByHash returns: the token, plus the three fields the
    /// OWNING ACCOUNT contributes to the reset flow.
    ///
    /// A dedicated type rather than bolting Username/Email/IsActive onto
    /// PasswordResetToken. The model is the token and nothing else, it is
    /// constructed by the INSERT path too (where those three fields do not
    /// exist yet), and widening it would let any caller persist a half-populated
    /// account. Keeping the account data in a read-only projection makes it
    /// impossible to write a token row that claims a username the user does not
    /// have. It is returned, never constructed by the DAL's write paths.
    /// </summary>
    public class ResetTokenLookup
    {
        public PasswordResetToken Token { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }

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

        /// <summary>
        /// Password-reset lookup, by the address the reset mail is sent to.
        /// Same column list as SelectByUsername MINUS PasswordHash and
        /// PasswordSalt: the reset flow needs to know who the address belongs
        /// to and whether the account may still be used, and has no need for
        /// the credentials themselves. Same reasoning as SelectAll - there is
        /// no reason for this query to pull the password columns out of the
        /// database at all, and the narrower the row that reaches the reset
        /// code, the less there is to leak if this method is ever called from
        /// somewhere that is not a password reset.
        ///
        /// Users.Email carries a UNIQUE index, so at most one row can match.
        /// </summary>
        public User SelectByEmail(string email)
        {
            const string sql =
                "SELECT UserID, Username, Email, RoleID, IsActive, CreatedDate " +
                "FROM Users WHERE Email = @Email";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@Email", SqlDbType.NVarChar, 100, email);
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

        // ------------------------------------------------------------------
        // Password replacement, for AuthBLL.CompletePasswordReset.
        //
        // Deliberately narrow: it touches ONLY the two credential columns. It
        // does not re-activate a deactivated account (that stays an explicit
        // admin action via UpdateStatus) and it does not re-check the old
        // password, because the emailed token already is the proof of
        // authorisation - re-prompting for the old password would break the
        // flow for the exact people who need it.
        //
        // Returns rows affected rather than void, unlike UpdateStatus/UpdateRole,
        // because the caller is inside a transaction: 0 means the account was
        // deleted between the token lookup and this write, and the new password
        // must NOT land while the token is already stamped.
        // ------------------------------------------------------------------
        public int UpdatePassword(int userId, string hash, string salt)
        {
            const string sql =
                "UPDATE Users SET PasswordHash = @Hash, PasswordSalt = @Salt WHERE UserID = @UserId";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@Hash", SqlDbType.NVarChar, 256, hash);
                DbHelper.AddParam(cmd, "@Salt", SqlDbType.NVarChar, 256, salt);
                DbHelper.AddParam(cmd, "@UserId", SqlDbType.Int, 0, userId);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Transactional twin of UpdatePassword. Joins the caller's connection
        /// and transaction so the new password, the token stamp and the
        /// invalidation of the user's other tokens all commit or roll back
        /// together; opening a second connection here would write OUTSIDE that
        /// transaction and could deadlock against the Users row the
        /// transaction already holds.
        /// </summary>
        public int UpdatePassword(SqlConnection con, SqlTransaction tx, int userId, string hash, string salt)
        {
            const string sql =
                "UPDATE Users SET PasswordHash = @Hash, PasswordSalt = @Salt WHERE UserID = @UserId";
            SqlParameter[] parameters =
            {
                new SqlParameter("@Hash", SqlDbType.NVarChar, 256) { Value = hash },
                new SqlParameter("@Salt", SqlDbType.NVarChar, 256) { Value = salt },
                new SqlParameter("@UserId", SqlDbType.Int, 0) { Value = userId }
            };
            return DbHelper.ExecuteNonQuery(con, tx, sql, parameters);
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

        // ------------------------------------------------------------------
        // Password reset.
        //
        // Everything here is keyed on TokenHash, the base64 SHA-256 of the
        // token that was emailed - the token itself never reaches the database,
        // so no query below can be turned into a reset link by whoever reads
        // the table. See Helpers/PasswordResetToken for the reasoning.
        //
        // None of these methods decide anything. "Is this token still
        // redeemable?" and "may this account reset?" are business rules and
        // live in AuthBLL; this layer only reads and writes rows.
        // ------------------------------------------------------------------

        /// <summary>
        /// Records a new reset request and returns its new TokenID. Same
        /// INSERT-then-SCOPE_IDENTITY shape as Insert above.
        ///
        /// CreatedDate is taken from the model rather than DateTime.Now so the
        /// caller can stamp one "now" and derive ExpiresAt from it, which keeps
        /// the two columns describing the same instant.
        /// </summary>
        public int InsertResetToken(PasswordResetToken token)
        {
            const string sql =
                "INSERT INTO PasswordResetTokens (UserID, TokenHash, ExpiresAt, UsedAt, CreatedDate) " +
                "VALUES (@UserId, @TokenHash, @ExpiresAt, @UsedAt, @CreatedDate); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@UserId", SqlDbType.Int, 0, token.UserID);
                DbHelper.AddParam(cmd, "@TokenHash", SqlDbType.NVarChar, 256, token.TokenHash);
                DbHelper.AddParam(cmd, "@ExpiresAt", SqlDbType.DateTime, 0, token.ExpiresAt);
                DbHelper.AddParam(cmd, "@UsedAt", SqlDbType.DateTime, 0, token.UsedAt);
                DbHelper.AddParam(cmd, "@CreatedDate", SqlDbType.DateTime, 0, token.CreatedDate);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Looks a token up by its hash, or returns null when the hash matches
        /// nothing. SingleRow, because UX_PasswordResetTokens_TokenHash is
        /// UNIQUE - a second match would mean two live tokens share a hash,
        /// which the schema exists to prevent.
        ///
        /// JOINs Users to bring back the Username and Email the confirmation
        /// mail would be addressed with, and IsActive, so the caller can refuse
        /// a token whose account has since been deactivated without a second
        /// round trip. Neither of those is selected from the token row, which
        /// is why the result is a ResetTokenLookup rather than a
        /// PasswordResetToken.
        ///
        /// The lookup is a plain equality match on a 256-bit digest of 32 bytes
        /// of CSPRNG output: there is nothing to time and nothing to guess.
        /// </summary>
        public ResetTokenLookup SelectResetTokenByHash(string tokenHash)
        {
            const string sql =
                "SELECT t.TokenID, t.UserID, t.TokenHash, t.ExpiresAt, t.UsedAt, t.CreatedDate, " +
                "       u.Username, u.Email, u.IsActive " +
                "FROM PasswordResetTokens t " +
                "JOIN Users u ON u.UserID = t.UserID " +
                "WHERE t.TokenHash = @TokenHash";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@TokenHash", SqlDbType.NVarChar, 256, tokenHash);
                con.Open();
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!r.Read()) return null;
                    return new ResetTokenLookup
                    {
                        Token = MapResetToken(r),
                        Username = DbHelper.GetString(r, "Username"),
                        Email = DbHelper.GetString(r, "Email"),
                        IsActive = DbHelper.GetBool(r, "IsActive")
                    };
                }
            }
        }

        /// <summary>
        /// Spends one token. Returns rows affected, NOT void, and the
        /// "AND UsedAt IS NULL" is the whole point: redemption is
        /// compare-and-set, so two requests carrying the same valid link race
        /// here and exactly one of them gets 1. Without the guard both would
        /// report success and both would write a password; with it, the loser
        /// gets 0 and its caller aborts the transaction.
        /// </summary>
        public int MarkResetTokenUsed(int tokenId, DateTime when)
        {
            const string sql =
                "UPDATE PasswordResetTokens SET UsedAt = @UsedAt " +
                "WHERE TokenID = @TokenId AND UsedAt IS NULL";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@UsedAt", SqlDbType.DateTime, 0, when);
                DbHelper.AddParam(cmd, "@TokenId", SqlDbType.Int, 0, tokenId);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Transactional twin of MarkResetTokenUsed.</summary>
        public int MarkResetTokenUsed(SqlConnection con, SqlTransaction tx, int tokenId, DateTime when)
        {
            const string sql =
                "UPDATE PasswordResetTokens SET UsedAt = @UsedAt " +
                "WHERE TokenID = @TokenId AND UsedAt IS NULL";
            SqlParameter[] parameters =
            {
                new SqlParameter("@UsedAt", SqlDbType.DateTime, 0) { Value = when },
                new SqlParameter("@TokenId", SqlDbType.Int, 0) { Value = tokenId }
            };
            return DbHelper.ExecuteNonQuery(con, tx, sql, parameters);
        }

        /// <summary>
        /// Revokes every token this user still holds, by stamping UsedAt rather
        /// than deleting: the row stays as evidence that a reset was issued and
        /// spent, which is what an admin investigating a compromised mailbox
        /// actually wants to see.
        ///
        /// Two callers. RequestPasswordReset runs it first so a user asking for
        /// a new link is left with exactly ONE live token - otherwise every
        /// request ever mailed stays replayable until it expires on its own.
        /// CompletePasswordReset runs it last, to kill any other link that
        /// arrived before this one; the token it just redeemed is already
        /// stamped, so the "UsedAt IS NULL" guard excludes it and the call
        /// needs no special case for "this one".
        ///
        /// Expired tokens are left alone. Stamping them would say nothing new,
        /// and leaving UsedAt NULL on a row that timed out keeps "expired" and
        /// "revoked" distinguishable when the rows are read back.
        /// </summary>
        public int InvalidateOutstandingResetTokens(int userId, DateTime when)
        {
            const string sql =
                "UPDATE PasswordResetTokens SET UsedAt = @UsedAt " +
                "WHERE UserID = @UserId AND UsedAt IS NULL AND ExpiresAt > @UsedAt";

            using (var con = DbHelper.GetConnection())
            using (var cmd = DbHelper.CreateCommand(con, sql))
            {
                DbHelper.AddParam(cmd, "@UsedAt", SqlDbType.DateTime, 0, when);
                DbHelper.AddParam(cmd, "@UserId", SqlDbType.Int, 0, userId);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Transactional twin of InvalidateOutstandingResetTokens.</summary>
        public int InvalidateOutstandingResetTokens(SqlConnection con, SqlTransaction tx, int userId, DateTime when)
        {
            const string sql =
                "UPDATE PasswordResetTokens SET UsedAt = @UsedAt " +
                "WHERE UserID = @UserId AND UsedAt IS NULL AND ExpiresAt > @UsedAt";
            SqlParameter[] parameters =
            {
                new SqlParameter("@UsedAt", SqlDbType.DateTime, 0) { Value = when },
                new SqlParameter("@UserId", SqlDbType.Int, 0) { Value = userId }
            };
            return DbHelper.ExecuteNonQuery(con, tx, sql, parameters);
        }

        // Serves three different column lists: SelectByUsername selects the
        // password columns and no RoleName, SelectAll does the reverse, and
        // SelectByEmail selects neither. The HasColumn guards let one Map cover
        // all three and return null for whatever the query did not ask for,
        // instead of duplicating the mapping.
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

        // Only SelectResetTokenByHash reads a token row, and it always asks for
        // the full set, so unlike Map above this needs no HasColumn guards. It
        // does use GetNullableDate for UsedAt: NULL is the normal state of a
        // token that has not been spent, not an anomaly to be defaulted away.
        private PasswordResetToken MapResetToken(IDataRecord r)
        {
            return new PasswordResetToken
            {
                TokenID = DbHelper.GetInt(r, "TokenID"),
                UserID = DbHelper.GetInt(r, "UserID"),
                TokenHash = DbHelper.GetString(r, "TokenHash"),
                ExpiresAt = DbHelper.GetDate(r, "ExpiresAt"),
                UsedAt = DbHelper.GetNullableDate(r, "UsedAt"),
                CreatedDate = DbHelper.GetDate(r, "CreatedDate")
            };
        }
    }
}
