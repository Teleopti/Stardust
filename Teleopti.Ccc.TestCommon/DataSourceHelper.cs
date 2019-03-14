using System.Collections.Generic;
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
		public static IDataSource CreateDatabasesAndDataSource(IComponentContext container) =>
			CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeFromContainer(container));

		public static IDataSource CreateDatabasesAndDataSource(DataSourceFactoryFactory factory) =>
			CreateDatabasesAndDataSource(factory.Make());

		public static IDataSource CreateDatabasesAndDataSource(IDataSourcesFactory factory)
		{
			CreateDatabases();
			return makeDataSource(factory);
		}

		public static void CreateDatabases() =>
			new DatabaseTestHelper().CreateDatabases();

		public static IDataSource CreateDataSource(IComponentContext container) =>
			CreateDataSource(DataSourceFactoryFactory.MakeFromContainer(container));

		public static IDataSource CreateDataSource(DataSourceFactoryFactory factory) =>
			CreateDataSource(factory.Make());

		public static IDataSource CreateDataSource(IDataSourcesFactory factory) =>
			makeDataSource(factory);

		private static IDataSource makeDataSource(IDataSourcesFactory factory)
		{
			var dataSourceSettings = CreateDataSourceSettings(InfraTestConfigReader.ApplicationConnectionString(), null, InfraTestConfigReader.TenantName());
			return factory.Create(dataSourceSettings, InfraTestConfigReader.AnalyticsConnectionString());
		}

		public static IDictionary<string, string> CreateDataSourceSettings(
			string connectionString, 
			int? timeout,
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