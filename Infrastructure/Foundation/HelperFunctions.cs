using System.Data;
using System.Data.SqlClient;
using log4net;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public static class HelperFunctions
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(HelperFunctions));

		public static int BulkInsert(DataTable dataTable, string tableName, string connectionString)
		{
			if (dataTable == null)
				return 0;

			new BulkWriter().WriteWithRetries(dataTable, connectionString, tableName);
			logger.Debug($"Rows bulk-inserted into '{tableName}' : {dataTable.Rows.Count}");
			return dataTable.Rows.Count;
		}

		public static int ExecuteNonQuery(CommandType commandType, string commandText, SqlParameter[] parameterCollection, string connectionString)
		{
			var db = new DatabaseCommand(commandType, commandText, connectionString);
			int rowsAffected = db.ExecuteNonQuery(parameterCollection ?? new SqlParameter[]{});
			if (rowsAffected < 0) //when SET NOCOUNT ON is used inside SP
			{
				rowsAffected = 0;
			}
			logger.Debug($"Rows affected by command '{commandText}' : {rowsAffected}");
			return rowsAffected;
		}

		public static int ExecuteNonQueryMaintenance(CommandType commandType, string commandText, string connectionString)
		{
			var db = new DatabaseCommand(commandType, commandText, connectionString);

			var rowsAffected = db.ExecuteNonQueryMaintenance();
			if (rowsAffected < 0) //when SET NOCOUNT ON is used inside SP
			{
				rowsAffected = 0;
			}
			logger.Debug($"Rows affected by command '{commandText}' : {rowsAffected}");
			return rowsAffected;
		}

		public static DataSet ExecuteDataSet(CommandType commandType, string commandText, SqlParameter[] parameterCollection, string connectionString)
		{
			var db = new DatabaseCommand(commandType, commandText, connectionString);
			return db.ExecuteDataSet(parameterCollection ?? new SqlParameter[] { });
		}

		public static object ExecuteScalar(CommandType commandType, string commandText, string connectionString)
		{
			var db = new DatabaseCommand(commandType, commandText, connectionString);
			return db.ExecuteScalar();
		}

		public static object ExecuteScalar(CommandType commandType, string commandText, SqlParameter[] parameterCollection, string connectionString)
		{
			var db = new DatabaseCommand(commandType, commandText, connectionString);
			return db.ExecuteScalar(parameterCollection ?? new SqlParameter[]{});
		}

		public static void TruncateTable(string storedProcedureName, string connectionString)
		{
			logger.Debug("Before TruncateTable");
			ExecuteNonQuery(CommandType.StoredProcedure, storedProcedureName, null, connectionString);
		}
	}
}
