using System;
using System.Data;
using System.Data.SqlClient;
using log4net;

namespace Teleopti.Support.Security
{
	internal class TeleoptiAnalyticsDataUpdate : ICommandLineCommand
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));

		public int Execute(CommandLineArgument commandLineArgument)
		{
			Bug27986FactSchedule(commandLineArgument);
			return 0;
		}

		public int Bug27986FactSchedule(CommandLineArgument commandLineArgument)
		{
			const int buildNumber = -2798601;
			const string systemVersion = "fact_schedule";
			
			SqlTransaction transaction = null;
			using (SqlConnection connection = new SqlConnection(commandLineArgument.DestinationConnectionString))
			{
				connection.Open();

				if (patchApplied(connection, systemVersion, buildNumber))
					return 1;

				SqlCommand command;

				log.Debug("re-order columns in table mart.fact_schedule ...");
				using (command = connection.CreateCommand())
				{
					command.CommandTimeout = 600;
					command.CommandText = "select count(*) from mart.fact_schedule_old";
					var rowsLeft = (Int32)command.ExecuteScalar();
					log.Debug("Total number of rows to update: " + rowsLeft);
					var rows = 1;

					command.CommandText = "Exec mart.sys_move_fact_schedule_data";
					
					{
						while (rows > 0)
						{
							using (transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
							{
								command.Transaction = transaction;
								rows = command.ExecuteNonQuery();
								transaction.Commit();
							}
							rowsLeft = rowsLeft - rows;
							log.Debug("\tRows left to update: " + rowsLeft);
						}
						log.Debug("No more rows that can be moved. Left overs are: " + rowsLeft);
					}
				}
			updateDatabaseVersion(connection, systemVersion, buildNumber);
			}
		log.Debug("re-order columns in table mart.fact_schedule. Done!");
		return 0;
		}

		private void updateDatabaseVersion(SqlConnection connection, string systemVersion, int buildNumber)
		{
			log.Debug("Updating database version...");
			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture,
					"INSERT INTO dbo.DatabaseVersion (BuildNumber,SystemVersion,AddedDate,AddedBy) VALUES ({0},'{1}',GetDate(),'{2}')",
					buildNumber,
					systemVersion,
					Environment.UserName);
				command.ExecuteNonQuery();
			}
			log.Debug("Done updating database version.");
		}

		private bool patchApplied(SqlConnection connection, string systemVersion, int buildNumber)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture,
					"SELECT COUNT(*) FROM dbo.DatabaseVersion WHERE BuildNumber={0} AND SystemVersion='{1}'",
					buildNumber, systemVersion);
				var versionCount = (int) command.ExecuteScalar();
				if (versionCount > 0)
				{
					return true;
				}
				else
					return false;
			}
		}
	}
}