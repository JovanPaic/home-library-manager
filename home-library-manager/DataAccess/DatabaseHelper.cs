using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading.Tasks;

namespace home_library_manager.DataAccess
{
    /// <summary>
    /// Provides async helper methods to interact with the SQL Server database.
    /// </summary>
    public static class DatabaseHelper
    {
        private static readonly string connectionString =
            ConfigurationManager.ConnectionStrings["DB"].ConnectionString;

        /// <summary>
        /// Returns a new SqlConnection using the configured connection string.
        /// </summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        /// <summary>
        /// Executes an async SELECT query and returns the result as a DataTable.
        /// </summary>
        public static DataTable ExecuteSelect(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (var con = GetConnection())
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = 30;
                    if (parameters != null)
                        AddParameters(cmd, parameters);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing SELECT query.", ex);
            }
        }

        /// <summary>
        /// Executes an async INSERT, UPDATE, or DELETE query.
        /// </summary>
        public static int ExecuteNonQuery(string query, Dictionary<string, object> parameters)
        {
            try
            {
                using (var con = GetConnection())
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = 30;
                    if (parameters != null)
                        AddParameters(cmd, parameters);

                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing non-query.", ex);
            }
        }

        /// <summary>
        /// Executes an async scalar query and returns the first column of the first row.
        /// </summary>
        public static object ExecuteScalar(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (var con = GetConnection())
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = 30;
                    if (parameters != null)
                        AddParameters(cmd, parameters);

                    con.Open();
                    return  cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing scalar query.", ex);
            }
        }

        /// <summary>
        /// Executes an async query with multiple commands as a transaction.
        /// Rolls back if any command fails.
        /// </summary>
        public static  void ExecuteTransaction(List<(string Query, Dictionary<string, object> Parameters)> commands)
        {
            using (var con = GetConnection())
            {
                con.Open();

                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        foreach (var (query, parameters) in commands)
                        {
                            using (var cmd = new SqlCommand(query, con, tran))
                            {
                                cmd.CommandTimeout = 30;
                                if (parameters != null)
                                    AddParameters(cmd, parameters);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw new Exception("Error executing transaction. All changes rolled back.", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Adds parameters to a SqlCommand object.
        /// </summary>
        private static void AddParameters(SqlCommand cmd, Dictionary<string, object> parameters)
        {
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }
    }
}
