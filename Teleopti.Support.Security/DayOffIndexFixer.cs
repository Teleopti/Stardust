using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using log4net;

namespace Teleopti.Support.Security
{
	internal class DayOffIndexFixer : ICommandLineCommand
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(DayOffIndexFixer));
		private const int specialVersion = 458;

		public int Execute(IDatabaseArguments databaseArguments)
		{
			using (var appConnection = new SqlConnection(databaseArguments.ApplicationDbConnectionString))
			using (var analyticsConnection = new SqlConnection(databaseArguments.AnalyticsDbConnectionString))
			{
				analyticsConnection.Open();

				if (isFixApplied(analyticsConnection))
					return 1;
				appConnection.Open();

				//Open transaction
				var transaction = analyticsConnection.BeginTransaction(IsolationLevel.ReadCommitted);

				// Drop the previous index
				executeNonQuery(analyticsConnection, transaction, @"
					IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME ='AK_day_off_code')
					BEGIN
						ALTER TABLE [mart].[dim_day_off] DROP CONSTRAINT [AK_day_off_code]
					END");

				// Update ContractDayOffs
				executeNonQuery(analyticsConnection, transaction, @"
					UPDATE mart.dim_day_off
						SET day_off_code = '00000000-0000-0000-0000-000000000000'
					WHERE day_off_name = 'ContractDayOff' 
						and day_off_shortname = 'CD'
						and day_off_code<>'00000000-0000-0000-0000-000000000000'");

				executeNonQuery(analyticsConnection, transaction, @"
					IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME ='AK_day_off_code_business_unit_id')
					BEGIN
						ALTER TABLE [mart].[dim_day_off] ADD  CONSTRAINT [AK_day_off_code_business_unit_id] UNIQUE NONCLUSTERED 
						(
							[day_off_code] ASC,
							[business_unit_id] ASC
						)
					END");

				setApplied(analyticsConnection, transaction);

				//Commit!
				log.Debug("Committing transaction...");
				transaction.Commit();
				log.Debug("Transaction successfully committed! Done.");
			}
			return 0;
		}

		private static void executeNonQuery(SqlConnection connection, SqlTransaction transaction, string query)
		{
			var command = connection.CreateCommand();
			command.CommandText = query;
			command.Transaction = transaction;
			command.ExecuteNonQuery();
		}

		private static void setApplied(SqlConnection connection, SqlTransaction transaction)
		{
			log.Debug("Updating database version...");
			executeNonQuery(connection, transaction, string.Format(CultureInfo.InvariantCulture,
				$"INSERT INTO dbo.DatabaseVersion (BuildNumber,SystemVersion,AddedDate,AddedBy) VALUES ('{-specialVersion}','8.4.{specialVersion}.1',GetDate(),'{0}')",
				Environment.UserName));
			log.Debug("Done updating database version.");
		}

		private static bool isFixApplied(SqlConnection connection)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = $"SELECT COUNT(*) FROM dbo.DatabaseVersion WHERE BuildNumber=('{-specialVersion}')";
				var versionCount = (int)command.ExecuteScalar();
				if (versionCount > 0)
					return true;
			}
			return false;
		}
	}
}