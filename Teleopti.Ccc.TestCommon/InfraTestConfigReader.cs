using System;
using System.Configuration;
using System.Data.SqlClient;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.TestCommon
{
	public static class InfraTestConfigReader
	{
		public static string InvalidConnectionString => @"Data Source=nakenjanne;Initial Catalog=Demoreg_TeleoptiCCC7;User Id=sa;Password=cadadi;Connect Timeout=1;";
		public static string SqlAuthString => ConfigurationManager.AppSettings["InfraTest.SqlAuthString"];
		public static string ApplicationDatabaseName() => new SqlConnectionStringBuilder(ApplicationConnectionString()).InitialCatalog;
		public static string ToggleMode => ConfigurationManager.AppSettings["InfraTest.ToggleMode"];
		public static string DatabaseBackupLocation => ConfigurationManager.AppSettings["InfraTest.DatabaseBackupLocation"];
		public static string AnalyticsDatabaseName() => new SqlConnectionStringBuilder(AnalyticsConnectionString()).InitialCatalog;

		private static readonly PerTestWorker<string> suffixPerWorkerId = new PerTestWorker<string>();

		public static string TenantName() => augmentName("TestData");
		public static string ApplicationConnectionString() => augmentConnectionString(ConfigurationManager.AppSettings["InfraTest.ConnectionString"]);
		public static string AnalyticsConnectionString() => augmentConnectionString(ConfigurationManager.AppSettings["InfraTest.AnalyticsConnectionString"]);
		public static string AggConnectionString() => augmentConnectionString(getAggConnectionString());

		private static string getAggConnectionString()
		{
			var builder = new SqlConnectionStringBuilder(ConfigurationManager.AppSettings["InfraTest.AnalyticsConnectionString"]);
			builder.InitialCatalog = builder.InitialCatalog.Contains("Analytics") ? 
				builder.InitialCatalog.Replace("Analytics", "Agg") : 
				"InfraTest_Agg";
			return builder.ToString();
		}

		private static string augmentConnectionString(string connectionString)
		{
			if (!TestContext.CurrentContext.isParallel())
				return connectionString;
			var c = new SqlConnectionStringBuilder(connectionString);
			c.InitialCatalog = augmentName(c.InitialCatalog);
			return c.ToString();
		}

		private static string augmentName(string name)
		{
			if (TestContext.CurrentContext.isParallel())
			{
				if (suffixPerWorkerId.Value == null)
					suffixPerWorkerId.Set(() => suffixPerWorkerId.WorkerCount().ToString());
				return name + "_" + suffixPerWorkerId.Value;
			}

			return name;
		}
	}
}