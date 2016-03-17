using System;
using System.Data.SqlClient;
using log4net;
using Manager.Integration.Test.Annotations;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;

namespace Manager.Integration.Test.Helpers
{
	public static class DatabaseHelper
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (DatabaseHelper));

		public static void TryClearDatabase(string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				Logger.ErrorWithLineNumber("Invalid connection string value.");

				throw new ArgumentNullException("connectionString");
			}

			Logger.DebugWithLineNumber("Start truncating database tables.");

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();

				//-------------------------------------------------
				// Truncate table Stardust.JobDefinitions.
				//-------------------------------------------------
				using (var command = new SqlCommand("truncate table Stardust.JobDefinitions",
				                                    connection))
				{
					Logger.DebugWithLineNumber("Start: " + command.CommandText);

					command.ExecuteNonQuery();

					Logger.DebugWithLineNumber("Finished: " + command.CommandText);
				}

				//-------------------------------------------------
				// Truncate table Stardust.JobHistory.
				//-------------------------------------------------
				using (var command = new SqlCommand("truncate table Stardust.JobHistory",
				                                    connection))
				{
					Logger.DebugWithLineNumber("Start: " + command.CommandText);

					command.ExecuteNonQuery();

					Logger.DebugWithLineNumber("Finished: " + command.CommandText);
				}

				//-------------------------------------------------
				// Truncate table Stardust.JobHistoryDetail.
				//-------------------------------------------------
				using (var command = new SqlCommand("truncate table Stardust.JobHistoryDetail",
				                                    connection))
				{
					Logger.DebugWithLineNumber("Start: " + command.CommandText);

					command.ExecuteNonQuery();

					Logger.DebugWithLineNumber("Finished: " + command.CommandText);
				}

				//-------------------------------------------------
				// Truncate table Stardust.WorkerNodes.
				//-------------------------------------------------
				using (var command = new SqlCommand("truncate table Stardust.WorkerNodes",
				                                    connection))
				{
					Logger.DebugWithLineNumber("Start: " + command.CommandText);

					command.ExecuteNonQuery();

					Logger.DebugWithLineNumber("Finished: " + command.CommandText);
				}

				connection.Close();

				Logger.DebugWithLineNumber("Finished truncating database tables.");
			}
		}
	}
}