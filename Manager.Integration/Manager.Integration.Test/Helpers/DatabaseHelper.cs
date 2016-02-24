using System;
using System.Data.SqlClient;
using log4net;

namespace Manager.Integration.Test.Helpers
{
	public static class DatabaseHelper
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (DatabaseHelper));

		public static void TryClearDatabase(string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				throw new ArgumentNullException("connectionString");
			}

			LogHelper.LogDebugWithLineNumber("Start.", Logger);

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();

				using (var command = new SqlCommand("truncate table Stardust.JobDefinitions",
					connection))
				{
					LogHelper.LogDebugWithLineNumber(command.CommandText, Logger);

					command.ExecuteNonQuery();
				}

				using (var command = new SqlCommand("truncate table Stardust.JobHistory",
					connection))
				{
					LogHelper.LogDebugWithLineNumber(command.CommandText, Logger);

					command.ExecuteNonQuery();
				}

				using (var command = new SqlCommand("truncate table Stardust.JobHistoryDetail",
					connection))
				{
					LogHelper.LogDebugWithLineNumber(command.CommandText, Logger);

					command.ExecuteNonQuery();
				}

				using (var command = new SqlCommand("truncate table Stardust.WorkerNodes",
					connection))
				{
					LogHelper.LogDebugWithLineNumber(command.CommandText, Logger);

					command.ExecuteNonQuery();
				}

				connection.Close();
				LogHelper.LogDebugWithLineNumber("Stop.", Logger);
			}
		}
	}
}