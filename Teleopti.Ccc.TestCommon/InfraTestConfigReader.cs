using System;
using System.Collections.Concurrent;
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

		// Using this before we change the way we have information about where agg database. This works for tests.
		private static string getAggConnectionString()
		{
			var aggName = "InfraTest_Agg";
			var analyticsConnectionString = ConfigurationManager.AppSettings["InfraTest.AnalyticsConnectionString"];
			const string initCatString = "Initial Catalog=";
			var firstIndex = analyticsConnectionString.IndexOf(initCatString, StringComparison.Ordinal) + initCatString.Length;
			var lastIndex = analyticsConnectionString.IndexOf(";", firstIndex, StringComparison.Ordinal);
			lastIndex = lastIndex == -1 ? analyticsConnectionString.Length : lastIndex;
			return $"{analyticsConnectionString.Substring(0, firstIndex)}{aggName}{analyticsConnectionString.Substring(lastIndex)}";
		}
		
		private static readonly ConcurrentDictionary<string, string> suffixPerWorkerId = new ConcurrentDictionary<string, string>();

		public static string TenantName() => augmentName("TestData");
		public static string ApplicationConnectionString() => augmentNameConnectionString(ConfigurationManager.AppSettings["InfraTest.ConnectionString"]);
		public static string AnalyticsConnectionString() => augmentNameConnectionString(ConfigurationManager.AppSettings["InfraTest.AnalyticsConnectionString"]);
		public static string AggConnectionString() => augmentNameConnectionString(getAggConnectionString());

		private static string augmentNameConnectionString(string connectionString)
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
				var workerId = TestContext.CurrentContext.WorkerId;
				var suffix = suffixPerWorkerId.GetOrAdd(workerId, x => suffixPerWorkerId.Count.ToString());
				return name + "_" + suffix;
			}

			return name;
		}
	}
}