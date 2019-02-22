﻿using System.Collections.Generic;
using System.Globalization;
using Autofac;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.TestCommon
{
	public static class DataSourceHelper
	{
		public const string TenantName = TestTenantName.Name;

		public static IDataSource CreateDatabasesAndDataSource(IComponentContext container, string name = TenantName) =>
			CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeFromContainer(container), name);

		public static IDataSource CreateDatabasesAndDataSource(DataSourceFactoryFactory factory, string name = TenantName) =>
			CreateDatabasesAndDataSource(factory.Make(), name);

		public static IDataSource CreateDatabasesAndDataSource(IDataSourcesFactory factory, string name = TenantName)
		{
			CreateDatabases(name);
			return makeDataSource(factory, name);
		}

		public static void CreateDatabases(string name = TenantName) =>
			new DatabaseTestHelper().CreateDatabases(name);

		public static IDataSource CreateDataSource(IComponentContext container) =>
			CreateDataSource(DataSourceFactoryFactory.MakeFromContainer(container));

		public static IDataSource CreateDataSource(DataSourceFactoryFactory factory) =>
			CreateDataSource(factory.Make());

		public static IDataSource CreateDataSource(IDataSourcesFactory factory) =>
			makeDataSource(factory, TenantName);

		private static IDataSource makeDataSource(IDataSourcesFactory factory, string name)
		{
			var dataSourceSettings = CreateDataSourceSettings(InfraTestConfigReader.ApplicationConnectionString(), null, name);
			return factory.Create(dataSourceSettings, InfraTestConfigReader.AnalyticsConnectionString());
		}

		public static IDictionary<string, string> CreateDataSourceSettings(string connectionString, int? timeout,
			string sessionFactoryName)
		{
			var dictionary = new Dictionary<string, string>();
			if (sessionFactoryName != null)
				dictionary[Environment.SessionFactoryName] = sessionFactoryName;
			dictionary[Environment.Dialect] = typeof(MsSql2008Dialect).AssemblyQualifiedName;
			dictionary[Environment.ConnectionString] = connectionString;
			dictionary[Environment.SqlExceptionConverter] = typeof(SqlServerExceptionConverter).AssemblyQualifiedName;
			if (timeout.HasValue)
				dictionary[Environment.CommandTimeout] = timeout.Value.ToString(CultureInfo.CurrentCulture);
			return dictionary;
		}


		public static void BackupApplicationDatabase(int dataHash) => new DatabaseTestHelper().BackupApplicationDatabase(dataHash);
		public static void BackupApplicationDatabaseBySql(string path, int dataHash) => new DatabaseTestHelper().BackupApplicationDatabaseBySql(path, dataHash);
		public static void RestoreApplicationDatabase(int dataHash) => new DatabaseTestHelper().RestoreApplicationDatabase(dataHash);
		public static bool TryRestoreApplicationDatabaseBySql(string path, int dataHash) => new DatabaseTestHelper().TryRestoreApplicationDatabaseBySql(path, dataHash);

		public static void BackupAnalyticsDatabase(int dataHash) => new DatabaseTestHelper().BackupAnalyticsDatabase(dataHash);
		public static void BackupAnalyticsDatabaseBySql(string path, int dataHash) => new DatabaseTestHelper().BackupAnalyticsDatabaseBySql(path, dataHash);
		public static void RestoreAnalyticsDatabase(int dataHash) => new DatabaseTestHelper().RestoreAnalyticsDatabase(dataHash);
		public static bool TryRestoreAnalyticsDatabaseBySql(string path, int dataHash) => new DatabaseTestHelper().TryRestoreAnalyticsDatabaseBySql(path, dataHash);
		public static void ClearAnalyticsData() => new DatabaseTestHelper().ClearAnalyticsData();

		public static void CreateFirstTenantAdminUser() => new DatabaseTestHelper().CreateFirstTenantAdminUser();
	}
}