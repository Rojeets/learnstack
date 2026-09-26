using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace TechStackLearningHub.Web.Data_Access_Layer
{
    /// <summary>
    /// All ADO.NET plumbing lives here. No business rules - this class only
    /// knows how to talk to SQL Server.
    ///
    /// Shape: repositories open their own connection with GetConnection, build
    /// a command with CreateCommand, add parameters with AddParam, execute,
    /// and map rows with the typed Get* accessors below. Connections are
    /// opened per call via `using`, never held in a field.
    ///
    /// This class is the ONLY holder of the connection string.
    /// </summary>
    public static class DbHelper
    {
        private static readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["TechStackDb"].ConnectionString;

        /// <summary>Creates but does NOT open. Caller wraps in using.</summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static SqlCommand CreateCommand(SqlConnection con, string sql)
        {
            var cmd = new SqlCommand(sql, con);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 30;
            return cmd;
        }

        /// <summary>Adds a parameter, translating C# null into database NULL.</summary>
        public static void AddParam(SqlCommand cmd, string name, object value)
        {
            cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        /// <summary>
        /// Typed overload. Worth it for NVARCHAR columns: AddWithValue infers
        /// the type from the runtime value, so a short string sent to an
        /// NVARCHAR(300) column can produce a different cached execution plan
        /// than a long one, which is a classic plan-cache-thrash source.
        /// </summary>
        public static void AddParam(SqlCommand cmd, string name, SqlDbType type, int size, object value)
        {
            var p = new SqlParameter(name, type, size) { Value = value ?? DBNull.Value };
            cmd.Parameters.Add(p);
        }

        // ------------------------------------------------------------------
        // Row accessors. These exist so a Map method never has to write
        // `row["X"] == DBNull.Value ? null : row["X"].ToString()` by hand, and
        // so a NULL never turns into the string "Null".
        // ------------------------------------------------------------------

        public static string GetString(IDataRecord r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? null : r.GetString(i);
        }

        public static int GetInt(IDataRecord r, string col, int fallback = 0)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? fallback : r.GetInt32(i);
        }

        public static int? GetNullableInt(IDataRecord r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? (int?)null : r.GetInt32(i);
        }

        public static bool GetBool(IDataRecord r, string col, bool fallback = false)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? fallback : r.GetBoolean(i);
        }

        public static decimal GetDecimal(IDataRecord r, string col, decimal fallback = 0m)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? fallback : r.GetDecimal(i);
        }

        public static DateTime GetDate(IDataRecord r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? DateTime.MinValue : r.GetDateTime(i);
        }

        public static DateTime? GetNullableDate(IDataRecord r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? (DateTime?)null : r.GetDateTime(i);
        }

        /// <summary>
        /// True when this result set actually selected the named column.
        ///
        /// Needed whenever one Map method serves several queries that select
        /// different column lists - GetOrdinal throws IndexOutOfRangeException
        /// on a column the query never asked for. Guarding with HasColumn lets a
        /// single Map cover both shapes and return a default for the missing
        /// one, instead of maintaining two nearly identical Map methods.
        /// </summary>
        public static bool HasColumn(IDataRecord r, string col)
        {
            for (int i = 0; i < r.FieldCount; i++)
            {
                if (r.GetName(i).Equals(col, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        // ------------------------------------------------------------------
        // Transactions
        // ------------------------------------------------------------------

        /// <summary>
        /// Runs work on one connection inside one transaction, committing on
        /// normal return and rolling back on any exception.
        ///
        /// Only used by quiz grading, where the Results insert and the Progress
        /// updates must both land or neither may: a passing score with no
        /// progress credit is a half-written state.
        /// </summary>
        public static void ExecuteInTransaction(Action<SqlConnection, SqlTransaction> work)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        work(conn, tx);
                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Connection/transaction-aware primitives. Used ONLY by the
        /// repository overloads that take (conn, tx) so their work joins the
        /// caller's transaction instead of opening a second connection that
        /// would run outside it.
        /// </summary>
        internal static int ExecuteNonQuery(SqlConnection conn, SqlTransaction tx, string sql, object[] parameters)
        {
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                if (parameters != null)
                    foreach (var p in parameters) cmd.Parameters.Add(p);
                return cmd.ExecuteNonQuery();
            }
        }

        internal static object ExecuteScalar(SqlConnection conn, SqlTransaction tx, string sql, object[] parameters)
        {
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                if (parameters != null)
                    foreach (var p in parameters) cmd.Parameters.Add(p);
                return cmd.ExecuteScalar();
            }
        }
    }
}
