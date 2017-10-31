using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using log4net;

namespace Teleopti.Support.Security
{
	internal class AnalyticsReportableScenarioFixer : CommandLineCommandWithFixChecker
	{
		public AnalyticsReportableScenarioFixer():base(595)
		{
		}
		
		private static readonly ILog log = LogManager.GetLogger(typeof(AnalyticsReportableScenarioFixer));
		
		public override int Execute(IDatabaseArguments databaseArguments)
		{
			using (var appConnection = new SqlConnection(databaseArguments.ApplicationDbConnectionString))
			using (var analyticsConnection = new SqlConnection(databaseArguments.AnalyticsDbConnectionString))
			{
				analyticsConnection.Open();

				if (isFixApplied(analyticsConnection))
					return 1;
				
				appConnection.Open();
				
				var applicationScenarios = loadApplicationScenarios(appConnection);
				var nonReportableScenarios = string.Join(",", applicationScenarios.Where(x => !x.EnableReporting).Select(x => $"'{x.Id}'"));
				var transaction = analyticsConnection.BeginTransaction(IsolationLevel.ReadCommitted);

				if (!string.IsNullOrEmpty(nonReportableScenarios))
				{
					log.Debug("Updating dim_scenario...");
					executeNonQuery(analyticsConnection, transaction,
						$"update mart.dim_scenario set is_deleted = 1 where scenario_code in ({nonReportableScenarios})");
				}
				
				setApplied(analyticsConnection, transaction);
				
				log.Debug("Committing transaction...");
				transaction.Commit();
				log.Debug("Transaction successfully committed! Done.");
			}

			return 0;
		}
		
		private static IEnumerable<AppScenario> loadApplicationScenarios(SqlConnection connection)
		{
			log.Debug("Load scenarios from application");
			var appScenarios = new List<AppScenario>();
			using (var scenarios = new SqlDataAdapter("SELECT * FROM [dbo].[Scenario]", connection))
			{
				using (var ds = new DataSet())
				{
					ds.Locale = CultureInfo.InvariantCulture;
					scenarios.Fill(ds);

					appScenarios.AddRange(ds.Tables[0].Rows.Cast<DataRow>().Select(row => new AppScenario(row)));
				}
			}
			return appScenarios;
		}

		internal class AppScenario
		{
			public AppScenario(DataRow row)
			{
				Id = (Guid) row["Id"];
				EnableReporting = (bool) row["EnableReporting"];
			}

			public Guid Id { get; set; }
			public bool EnableReporting { get; set; }
		}
	}
}