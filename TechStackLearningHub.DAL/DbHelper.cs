using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace TechStackLearningHub.DAL
{
    // The ONLY class allowed to hold the connection string.
    public static class DbHelper
    {
        private static readonly string ConnStr =
            ConfigurationManager.ConnectionStrings["TechStackDb"].ConnectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnStr);
        }

        public static int ExecuteNonQuery(string sql, SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static DataTable ExecuteQuery(string sql, SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                var table = new DataTable();
                using (var adapter = new SqlDataAdapter(cmd))
                    adapter.Fill(table);
                return table;
            }
        }

        public static object ExecuteScalar(string sql, SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

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

        // Connection/transaction-aware primitives used ONLY by repositories'
        // transactional overloads so work runs on the ambient SqlTransaction.
        internal static int ExecuteNonQuery(SqlConnection conn, SqlTransaction tx, string sql, SqlParameter[] parameters)
        {
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        internal static object ExecuteScalar(SqlConnection conn, SqlTransaction tx, string sql, SqlParameter[] parameters)
        {
            using (var cmd = new SqlCommand(sql, conn, tx))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }
    }
}