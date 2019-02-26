using System;
using System.Collections.Generic;
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

		private static readonly Dictionary<string, string> suffixPerWorkerId = new Dictionary<string, string>();

		public static string TenantName() => augmentName("TestData");
		public static string ApplicationConnectionString() => augmentConnectionString(ConfigurationManager.AppSettings["InfraTest.ConnectionString"]);
		public static string AnalyticsConnectionString() => augmentConnectionString(ConfigurationManager.AppSettings["InfraTest.AnalyticsConnectionString"]);
		public static string AggConnectionString() => augmentConnectionString(getAggConnectionString());

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
				var workerId = TestContext.CurrentContext.WorkerId;
				if (!suffixPerWorkerId.ContainsKey(workerId))
					lock (suffixPerWorkerId)
						if (!suffixPerWorkerId.ContainsKey(workerId))
							suffixPerWorkerId[workerId] = suffixPerWorkerId.Count.ToString();
				var suffix = suffixPerWorkerId[workerId];
				return name + "_" + suffix;
			}

			return name;
		}
	}
}