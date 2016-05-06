﻿using System;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.CompilerServices;
using log4net;
using Manager.IntegrationTest.Console.Host.Log4Net;

namespace Manager.Integration.Test.Database
{
	public static class DatabaseHelper
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (DatabaseHelper));


		public static void AddPerformanceData(string connectionString, 
											  string description,
											  DateTime started,
											  DateTime ended,
											  [CallerFilePath] string file = "",
											  [CallerMemberName] string member = "")
		{
			var filename = Path.GetFileName(file) + "_" + member;
			AddPerformanceData(connectionString, filename, description, started, ended);
		}

		public static void AddPerformanceData(string connectionString,
		                                      string name,
		                                      string description,
		                                      DateTime started,
											  DateTime ended)
		{
			const string insertCommandText =
				@"INSERT INTO [Stardust].[PerformanceTest]
						  (Name, 
						   Description, 
						   Started,
						   Ended,
						   ElapsedInSeconds,
						   ElapsedInMinutes)
				 VALUES
						  (@Name, 
						   @Description, 
						   @Started,
						   @Ended,
						   @ElapsedInSeconds,
						   @ElapsedInMinutes)";

			using (var sqlConnection = new SqlConnection(connectionString))
			{
				sqlConnection.Open();

				using (var sqlCommand = new SqlCommand(insertCommandText, sqlConnection))
				{
					sqlCommand.Parameters.AddWithValue("@Name", name);
					sqlCommand.Parameters.AddWithValue("@Description", description);
					sqlCommand.Parameters.AddWithValue("@Started", started);
					sqlCommand.Parameters.AddWithValue("@Ended", ended);
					sqlCommand.Parameters.AddWithValue("@ElapsedInSeconds", (ended - started).TotalSeconds);
					sqlCommand.Parameters.AddWithValue("@ElapsedInMinutes", (ended - started).TotalMinutes);

					sqlCommand.ExecuteNonQuery();
				}
			}
		}
		
		public static void TryClearDatabase(string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				Logger.ErrorWithLineNumber("Invalid connection string value.");
				throw new ArgumentNullException("connectionString");
			}

			using (var sqlConnection = new SqlConnection(connectionString))
			{
				sqlConnection.Open();

				using (var sqlCommand = new SqlCommand("truncate table [Stardust].[JobQueue]",sqlConnection))
				{
					sqlCommand.ExecuteNonQuery();
				}
				using (var command = new SqlCommand("truncate table [Stardust].[Job]", sqlConnection))
				{
					command.ExecuteNonQuery();
				}
				using (var command = new SqlCommand("truncate table [Stardust].[JobDetail]", sqlConnection))
				{
					command.ExecuteNonQuery();
				}
				using (var command = new SqlCommand("truncate table [Stardust].[WorkerNode]", sqlConnection))
				{
					command.ExecuteNonQuery();
				}

				sqlConnection.Close();
			}
		}
	}
}