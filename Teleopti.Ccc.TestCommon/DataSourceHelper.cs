using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Autofac;
using Newtonsoft.Json;
using NHibernate.Dialect;
using NUnit.Framework;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Messaging.Client;
using Teleopti.Support.Library;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.TestCommon
{
	public static class DataSourceHelper
	{
		public const string TestTenantName = "TestData";

		public static IDataSource CreateDatabasesAndDataSource(IComponentContext container, string name = TestTenantName) => 
			CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeFromContainer(container), name);

		public static IDataSource CreateDatabasesAndDataSource(DataSourceFactoryFactory factory, string name = TestTenantName) =>
			CreateDatabasesAndDataSource(factory.Make(), name);

		public static IDataSource CreateDatabasesAndDataSource(string name = TestTenantName)
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new ConfigReader()) {FeatureToggle = "http://notinuse"})));
			builder.RegisterType<NoMessageSender>().As<IMessageSender>().SingleInstance();
			builder.RegisterType<FakeHangfireEventClient>().As<IHangfireEventClient>().SingleInstance();
			var container = builder.Build();
			return CreateDatabasesAndDataSource(container.Resolve<IDataSourcesFactory>(), name);
		}

		public static IDataSource CreateDatabasesAndDataSource(IDataSourcesFactory factory, string name = TestTenantName)
		{
			using (testDirectoryFix())
				CreateDatabases(name);
			return makeDataSource(factory, name);
		}
		
		public static void CreateDatabases(string name = TestTenantName)
		{
			createOrRestoreApplication(name);
			createOrRestoreAnalytics();
			createOrRestoreAgg();
		}

		public static IDataSource CreateDataSource(IComponentContext container) =>
			CreateDataSource(DataSourceFactoryFactory.MakeFromContainer(container));

		public static IDataSource CreateDataSource(DataSourceFactoryFactory factory) =>
			CreateDataSource(factory.Make());

		public static IDataSource CreateDataSource(IDataSourcesFactory factory) =>
			makeDataSource(factory, TestTenantName);
		
		private static IDataSource makeDataSource(IDataSourcesFactory factory, string name)
		{
			var dataSourceSettings = CreateDataSourceSettings(InfraTestConfigReader.ConnectionString, null, name);
			return factory.Create(dataSourceSettings, InfraTestConfigReader.AnalyticsConnectionString);
		}


		public static void BackupApplicationDatabase(int dataHash)
		{
			using (testDirectoryFix())
				backupByFileCopy(application(), dataHash);
		}

		public static void RestoreApplicationDatabase(int dataHash)
		{
			using (testDirectoryFix())
				restoreByFileCopy(application(), dataHash);
		}

		public static void BackupApplicationDatabaseBySql(string path, int dataHash)
		{
			var database = application();
			database.BackupBySql().Backup(path, database.BackupNameForBackup(dataHash));
		}

		public static bool TryRestoreApplicationDatabaseBySql(string path, int dataHash)
		{
			var database = application();
			return database.BackupBySql().TryRestore(path, database.BackupNameForRestore(dataHash));
		}


		public static void BackupAnalyticsDatabase(int dataHash)
		{
			using (testDirectoryFix())
				backupByFileCopy(analytics(), dataHash);
		}

		public static void RestoreAnalyticsDatabase(int dataHash)
		{
			using (testDirectoryFix())
				restoreByFileCopy(analytics(), dataHash);
		}

		public static void ClearAnalyticsData()
		{
			analytics().ConfigureSystem().CleanByAnalyticsProcedure();
		}

		public static void BackupAnalyticsDatabaseBySql(string path, int dataHash)
		{
			var database = analytics();
			database.BackupBySql().Backup(path, database.BackupNameForBackup(dataHash));
		}

		public static bool TryRestoreAnalyticsDatabaseBySql(string path, int dataHash)
		{
			var database = analytics();
			return database.BackupBySql().TryRestore(path, database.BackupNameForRestore(dataHash));
		}


		private static IDisposable testDirectoryFix()
		{
			var path = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			return new GenericDisposable(() => { Directory.SetCurrentDirectory(path); });
		}

		private static DatabaseHelper application()
		{
			return new DatabaseHelper(
				InfraTestConfigReader.ConnectionString,
				DatabaseType.TeleoptiCCC7,
				new DbManagerLog4Net("DbManager.Application")
			);
		}

		private static DatabaseHelper agg()
		{
			return new DatabaseHelper(
				InfraTestConfigReader.AggConnectionString,
				DatabaseType.TeleoptiCCCAgg,
				new DbManagerLog4Net("DbManager.Agg")
			);
		}

		private static DatabaseHelper analytics()
		{
			return new DatabaseHelper(
				InfraTestConfigReader.AnalyticsConnectionString,
				DatabaseType.TeleoptiAnalytics,
				new DbManagerLog4Net("DbManager.Analytics")
			);
		}

		private static void createOrRestoreApplication(string name)
		{
			var database = application();
			if (tryRestoreByFileCopy(database, 0))
			{
				database.ConfigureSystem().SetTenantConnectionInfo(name, database.ConnectionString, analytics().ConnectionString);
				return;
			}

			createDatabase(database);

			//would be better if dbmanager was called, but don't have the time right now....
			// eh, that thing that is called IS the db manager!
			application().ConfigureSystem().MergePersonAssignments();
			database.ConfigureSystem().PersistAuditSetting();
			database.ConfigureSystem().SetTenantConnectionInfo(name, database.ConnectionString, analytics().ConnectionString);

			backupByFileCopy(database, 0);
		}

		private static void createOrRestoreAnalytics()
		{
			var database = analytics();
			if (tryRestoreByFileCopy(database, 0))
				return;
			createDatabase(database);
			backupByFileCopy(database, 0);
		}

		private static void createOrRestoreAgg()
		{
			var database = agg();
			if (tryRestoreByFileCopy(database, 0))
				return;
			createDatabase(database);
			backupByFileCopy(database, 0);
		}

		private static void createDatabase(DatabaseHelper database)
		{
			database.CreateByDbManager();
			database.CreateSchemaByDbManager();
		}

		private static void backupByFileCopy(DatabaseHelper database, int dataHash)
		{
			var name = database.BackupNameForBackup(dataHash);
			var backup = database.BackupByFileCopy().Backup(name);
			File.WriteAllText(name, JsonConvert.SerializeObject(backup, Formatting.Indented));
		}

		private static bool tryRestoreByFileCopy(DatabaseHelper database, int dataHash)
		{
			// maybe it would be possible to attach it if a file exists but the database doesnt. but wth..
			if (!database.Tasks().Exists(database.DatabaseName))
				return false;

			var name = database.BackupNameForRestore(dataHash);
			if (!File.Exists(name))
				return false;

			var backup = JsonConvert.DeserializeObject<Backup>(File.ReadAllText(name));
			return database.BackupByFileCopy().TryRestore(backup);
		}

		private static void restoreByFileCopy(DatabaseHelper database, int dataHash)
		{
			var backup = JsonConvert.DeserializeObject<Backup>(File.ReadAllText(database.BackupNameForRestore(dataHash)));
			var result = database.BackupByFileCopy().TryRestore(backup);
			if (!result)
				throw new Exception("Restore failed!");
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


		public static void CreateFirstTenantAdminUser()
		{
			var database = application();
			database.ConfigureSystem().TryAddTenantAdminUser();
		}
	}

}