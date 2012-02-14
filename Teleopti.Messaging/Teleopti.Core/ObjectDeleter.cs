using System;
using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Core
{
    public class ObjectDeleter
    {
        private readonly string _connectionString;

        protected ObjectDeleter(string connectionString)
        {
            _connectionString = connectionString;
        }

        #pragma warning disable 1692

        /// <summary>
        /// Deletes records.
        /// </summary>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2009-03-29
        /// </remarks>
        public void DeleteRecords(string storedProcedure)
        {
            using (SqlConnection connection = ConnectionFactory.GetInstance(_connectionString).GetOpenConnection())
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = storedProcedure;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Deletes the added record.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="name">The name.</param>
        /// <param name="id">The id.</param>
        protected void DeleteAddedRecord(string commandText, string name, Int32 id)
        {
            using (SqlConnection connection = ConnectionFactory.GetInstance(_connectionString).GetOpenConnection())
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = commandText;
                    cmd.CommandType = CommandType.StoredProcedure; 
                    cmd.Connection = connection;
                    SqlParameter parameter = new SqlParameter(name, SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = id;
                    cmd.Parameters.Add(parameter);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Deletes the added record.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="name">The name.</param>
        /// <param name="id">The id.</param>
        protected void DeleteAddedRecord(string commandText, string name, Guid id)
        {
            using (SqlConnection connection = ConnectionFactory.GetInstance(_connectionString).GetOpenConnection())
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = commandText;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;
                    SqlParameter parameter = new SqlParameter(name, SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = id;
                    cmd.Parameters.Add(parameter);
                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}
