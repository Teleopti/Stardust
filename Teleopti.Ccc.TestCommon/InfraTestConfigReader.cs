using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.TestCommon
{
	public static class InfraTestConfigReader
	{
		public static string InvalidConnectionString => @"Data Source=nakenjanne;Initial Catalog=Demoreg_TeleoptiCCC7;User Id=sa;Password=cadadi;Connect Timeout=1;";
		public static string SqlServerName => ConfigurationManager.AppSettings["InfraTest.SqlServerName"];
		public static string SqlAuthString => ConfigurationManager.AppSettings["InfraTest.SqlAuthString"];
		public static string DatabaseName => ConfigurationManager.AppSettings["InfraTest.DatabaseName"];
		public static string ToggleMode => ConfigurationManager.AppSettings["InfraTest.ToggleMode"];
		public static string AnalyticsDatabaseName => ConfigurationManager.AppSettings["InfraTest.AnalyticsDatabaseName"];
		public static string DatabaseBackupLocation => ConfigurationManager.AppSettings["InfraTest.DatabaseBackupLocation"];

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

		private static ThreadLocal<databaseLock> threadDatabaseLock = new ThreadLocal<databaseLock>();

		private static IEnumerable<databaseLock> databaseLocks = new[]
		{
			new databaseLock("0"),
			new databaseLock("1"),
			new databaseLock("2"),
			new databaseLock("3"),
			new databaseLock("4"),
			new databaseLock("5"),
			new databaseLock("6"),
			new databaseLock("7"),
			new databaseLock("8"),
			new databaseLock("9"),
		};

		private class databaseLock
		{
			public string Name { get; }
			public string TenantName { get; }
			public string ApplicationConnectionString { get; }
			public string AnalyticsConnectionString { get; }
			public string AggConnectionString { get; }

			public databaseLock(string name)
			{
				Name = name;
				TenantName = "TestData_" + Name;
				ApplicationConnectionString = buildConnectionString(ConfigurationManager.AppSettings["InfraTest.ConnectionString"]);
				AnalyticsConnectionString = buildConnectionString(ConfigurationManager.AppSettings["InfraTest.AnalyticsConnectionString"]);
				AggConnectionString = buildConnectionString(getAggConnectionString());
			}

			private string buildConnectionString(string connectionString)
			{
				var c = new SqlConnectionStringBuilder(connectionString);
				c.InitialCatalog = c.InitialCatalog + "_" + Name;
				return c.ToString();
			}
		}

		public static void LockDatabases()
		{
			if (!TestContext.CurrentContext.isParallel())
				return;
			while (true)
			{
				var @lock = databaseLocks.FirstOrDefault(Monitor.TryEnter);
				if (@lock != null)
				{
					threadDatabaseLock.Value = @lock;
					return;
				}

				Thread.Sleep(100);
			}
		}

		public static void ReleaseDatabases()
		{
			if (threadDatabaseLock.Value != null)
				Monitor.Exit(threadDatabaseLock.Value);
			threadDatabaseLock.Value = null;
		}

		public static string TenantName() =>
			parallelCheck(() => threadDatabaseLock.Value?.TenantName ?? "TestData");

		public static string ApplicationConnectionString() =>
			parallelCheck(() => threadDatabaseLock.Value?.ApplicationConnectionString ?? ConfigurationManager.AppSettings["InfraTest.ConnectionString"]);

		public static string AnalyticsConnectionString() =>
			parallelCheck(() => threadDatabaseLock.Value?.AnalyticsConnectionString ?? ConfigurationManager.AppSettings["InfraTest.AnalyticsConnectionString"]);

		public static string AggConnectionString() =>
			parallelCheck(() => threadDatabaseLock.Value?.AggConnectionString ?? getAggConnectionString());

		private static T parallelCheck<T>(Func<T> func)
		{
			if (threadDatabaseLock.Value == null && TestContext.CurrentContext.isParallel())
				throw new Exception("Need to lock database properly when running tests in parallel");
			return func.Invoke();
		}
	}
}