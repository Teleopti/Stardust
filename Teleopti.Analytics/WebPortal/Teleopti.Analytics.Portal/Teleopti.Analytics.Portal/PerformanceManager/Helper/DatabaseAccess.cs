using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace Teleopti.Analytics.Portal.PerformanceManager.Helper
{
    public class DatabaseAccess
    {
        private readonly string _commandText;
        private readonly CommandType _commandType;
        private readonly string _connString;
        
        public DatabaseAccess(CommandType commandType, string commandText, string connectionString)
        {
            _connString = connectionString;
            _commandType = commandType;
            _commandText = commandText;
        }

        public object ExecuteScalar(IDictionary<string, object> parameters)
        {
	        using (var connection = grabConnection())
	        {
		        using (var cmd = connection.CreateCommand())
		        {
			        cmd.CommandText = _commandText;
			        cmd.CommandType = _commandType;

			        if (parameters != null)
			        {
				        foreach (var parameter in parameters)
				        {
							notNull(parameter.Key,parameter.Value);
					        cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
				        }
			        }

			        return cmd.ExecuteScalar();
		        }
	        }
        }

        private SqlConnection grabConnection()
        {
			var conn = new SqlConnection(_connString);
			conn.Open();
	        return conn;
        }

        private static void notNull(string parameterName, object parameterValue)
        {
            if (parameterValue == null)
            {
                string errMess = string.Format(CultureInfo.CurrentCulture,
                                               "Parameter '{0}' must not be null.",
                                               parameterName);
                throw new ArgumentNullException(parameterName, errMess);
            }
        }
    }
}