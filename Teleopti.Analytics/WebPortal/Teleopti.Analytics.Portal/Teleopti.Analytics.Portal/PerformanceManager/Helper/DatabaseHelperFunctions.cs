using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Analytics.Portal.PerformanceManager.Helper
{
    public static class DatabaseHelperFunctions
    {
        public static int ExecuteNonQuery(CommandType commandType, string commandText, ICollection<SqlParameter> parameterCollection, string connectionString)
        {
            using (DatabaseAccess databaseAccess = new DatabaseAccess(commandType, commandText, connectionString))
            {
                if (parameterCollection != null)
                {
                    foreach (SqlParameter parameter in parameterCollection)
                    {
                        databaseAccess.AddProcParameter(new SqlParameter(parameter.ParameterName, parameter.Value));
                    }
                }
                int rowsAffected = databaseAccess.ExecuteNonQuery();
                return rowsAffected;
            }
        }

        public static DataSet ExecuteDataSet(CommandType commandType, string commandText, ICollection<SqlParameter> parameterCollection, string connectionString)
        {
            using (DatabaseAccess databaseAccess = new DatabaseAccess(commandType, commandText, connectionString))
            {
                if (parameterCollection != null)
                {
                    foreach (SqlParameter parameter in parameterCollection)
                    {
                        databaseAccess.AddProcParameter(new SqlParameter(parameter.ParameterName, parameter.Value));
                    }
                }
                return databaseAccess.ExecuteDataSet();
            }
        }

        public static object ExecuteScalar(CommandType commandType, string commandText, string connectionString)
        {
            using (DatabaseAccess databaseAccess = new DatabaseAccess(commandType, commandText, connectionString))
            {
                return databaseAccess.ExecuteScalar();
            }
        }

        public static object ExecuteScalar(CommandType commandType, string commandText, ICollection<SqlParameter> parameterCollection, string connectionString)
        {
            using (DatabaseAccess databaseAccess = new DatabaseAccess(commandType, commandText, connectionString))
            {
                if (parameterCollection != null)
                {
                    foreach (SqlParameter parameter in parameterCollection)
                    {
                        databaseAccess.AddProcParameter(new SqlParameter(parameter.ParameterName, parameter.Value));
                    }
                }
                return databaseAccess.ExecuteScalar();
            }
        }
    }
}