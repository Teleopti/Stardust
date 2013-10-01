using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
	public static class HelperFunctions
	{
		public static int BulkInsert(DataTable dataTable, string tableName, string connectionString)
		{
			if (dataTable == null)
				return 0;

			BulkWriter.BulkWrite(dataTable, connectionString, tableName);
			Trace.WriteLine("Rows bulk-inserted into '" + tableName + "' : " + dataTable.Rows.Count);
			return dataTable.Rows.Count;
		}

		public static int ExecuteNonQuery(CommandType commandType, string commandText, ICollection<SqlParameter> parameterCollection, string connectionString)
		{
			var db = new DatabaseCommand(commandType, commandText, connectionString);
			if (parameterCollection != null)
			{
				foreach (SqlParameter parameter in parameterCollection)
				{
					db.AddProcParameter(new SqlParameter(parameter.ParameterName, parameter.Value));
				}
			}
			int rowsAffected = db.ExecuteNonQuery();
			Trace.WriteLine("Rows affected by command '" + commandText + "': " + rowsAffected);
			return rowsAffected;
        }

		public static DataSet ExecuteDataSet(CommandType commandType, string commandText, ICollection<SqlParameter> parameterCollection, string connectionString)
		{
			var db = new DatabaseCommand(commandType, commandText, connectionString);
			if (parameterCollection != null)
			{
				foreach (SqlParameter parameter in parameterCollection)
				{
					db.AddProcParameter(new SqlParameter(parameter.ParameterName, parameter.Value));
				}
			}
			return db.ExecuteDataSet();
		}

		public static object ExecuteScalar(CommandType commandType, string commandText, string connectionString)
		{
			var db = new DatabaseCommand(commandType, commandText, connectionString);
			return db.ExecuteScalar();
		}

		public static object ExecuteScalar(CommandType commandType, string commandText, ICollection<SqlParameter> parameterCollection, string connectionString)
		{
			var db = new DatabaseCommand(commandType, commandText, connectionString);
			if (parameterCollection != null)
			{
				foreach (SqlParameter parameter in parameterCollection)
				{
					db.AddProcParameter(new SqlParameter(parameter.ParameterName, parameter.Value));
				}
			}
			return db.ExecuteScalar();
		}

		public static void TruncateTable(string storedProcedureName, string connectionString)
		{
			Trace.WriteLine("Before TruncateTable");
			ExecuteNonQuery(CommandType.StoredProcedure, storedProcedureName, null, connectionString);
		}
	}
}
