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
			LogHelper.LogDebugWithLineNumber("Start.", Logger);

			if (string.IsNullOrEmpty(connectionString))
			{
				LogHelper.LogErrorWithLineNumber("Invalid connection string value.", Logger);

				throw new ArgumentNullException("connectionString");
			}

			LogHelper.LogDebugWithLineNumber("Start truncating database tables.", Logger);

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();

				//-------------------------------------------------
				// Truncate table Stardust.JobDefinitions.
				//-------------------------------------------------
				using (var command = new SqlCommand("truncate table Stardust.JobDefinitions",
				                                    connection))
				{
					LogHelper.LogDebugWithLineNumber("Start: " + command.CommandText, Logger);

					command.ExecuteNonQuery();

					LogHelper.LogDebugWithLineNumber("Finished: " + command.CommandText, Logger);
				}

				//-------------------------------------------------
				// Truncate table Stardust.JobHistory.
				//-------------------------------------------------
				using (var command = new SqlCommand("truncate table Stardust.JobHistory",
				                                    connection))
				{
					LogHelper.LogDebugWithLineNumber("Start: " + command.CommandText, Logger);

					command.ExecuteNonQuery();

					LogHelper.LogDebugWithLineNumber("Finished: " + command.CommandText, Logger);
				}

				//-------------------------------------------------
				// Truncate table Stardust.JobHistoryDetail.
				//-------------------------------------------------
				using (var command = new SqlCommand("truncate table Stardust.JobHistoryDetail",
				                                    connection))
				{
					LogHelper.LogDebugWithLineNumber("Start: " + command.CommandText, Logger);

					command.ExecuteNonQuery();

					LogHelper.LogDebugWithLineNumber("Finished: " + command.CommandText, Logger);
				}

				//-------------------------------------------------
				// Truncate table Stardust.WorkerNodes.
				//-------------------------------------------------
				using (var command = new SqlCommand("truncate table Stardust.WorkerNodes",
				                                    connection))
				{
					LogHelper.LogDebugWithLineNumber("Start: " + command.CommandText, Logger);

					command.ExecuteNonQuery();

					LogHelper.LogDebugWithLineNumber("Finished: " + command.CommandText, Logger);
				}

				connection.Close();

				LogHelper.LogDebugWithLineNumber("Finished truncating database tables.", Logger);

				LogHelper.LogDebugWithLineNumber("Finsihed.", Logger);
			}
		}
	}
}