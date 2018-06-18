using System;
using System.Configuration;

namespace Teleopti.Ccc.TestCommon
{
	public static class InfraTestConfigReader
	{
		public static string ConnectionString => ConfigurationManager.AppSettings["InfraTest.ConnectionString"];
		public static string InvalidConnectionString => @"Data Source=nakenjanne;Initial Catalog=Demoreg_TeleoptiCCC7;User Id=sa;Password=cadadi;Connect Timeout=1;";
		public static string AnalyticsConnectionString => ConfigurationManager.AppSettings["InfraTest.AnalyticsConnectionString"];
		public static string AggConnectionString => getAggConnectionString("InfraTest_Agg", AnalyticsConnectionString);
		public static string SqlServerName => ConfigurationManager.AppSettings["InfraTest.SqlServerName"];
		public static string SqlAuthString => ConfigurationManager.AppSettings["InfraTest.SqlAuthString"];
		public static string DatabaseName => ConfigurationManager.AppSettings["InfraTest.DatabaseName"];
		public static string ToggleMode => ConfigurationManager.AppSettings["InfraTest.ToggleMode"];
		public static string AnalyticsDatabaseName => ConfigurationManager.AppSettings["InfraTest.AnalyticsDatabaseName"];
		public static string DatabaseBackupLocation => ConfigurationManager.AppSettings["InfraTest.DatabaseBackupLocation"];

		// Using this before we change the way we have information about where agg database. This works for tests.
		private static string getAggConnectionString(string aggName, string analyticsConnectionString)
		{
			const string initCatString = "Initial Catalog=";
			var firstIndex = analyticsConnectionString.IndexOf(initCatString, StringComparison.Ordinal) + initCatString.Length;
			var lastIndex = analyticsConnectionString.IndexOf(";", firstIndex, StringComparison.Ordinal);
			lastIndex = lastIndex == -1 ? analyticsConnectionString.Length : lastIndex;
			return $"{analyticsConnectionString.Substring(0, firstIndex)}{aggName}{analyticsConnectionString.Substring(lastIndex)}";
		}
	}
}