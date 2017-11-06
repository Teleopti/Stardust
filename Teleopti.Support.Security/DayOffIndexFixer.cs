using System.Data;
using System.Data.SqlClient;
using log4net;

namespace Teleopti.Support.Security
{
	internal class DayOffIndexFixer : CommandLineCommandWithFixChecker
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(DayOffIndexFixer));

		public DayOffIndexFixer() : base(458)
		{
		}

		public override int Execute(IDatabaseArguments databaseArguments)
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
	}
}