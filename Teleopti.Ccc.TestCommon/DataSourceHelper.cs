using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using NHibernate.Dialect;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.TestCommon
{
	public static class DataSourceHelper
	{
		public const string TestTenantName = "TestData";

		public static IDataSource CreateDatabasesAndDataSource(ICurrentPersistCallbacks persistCallbacks)
		{
			return CreateDatabasesAndDataSource(persistCallbacks, TestTenantName);
		}

		public static IDataSource CreateDatabasesAndDataSource(ICurrentPersistCallbacks persistCallbacks, string name)
		{
			CreateDatabases(name);
			return makeDataSource(persistCallbacks, name);
		}
		
		public static void CreateDatabases()
		{
			CreateDatabases(TestTenantName);
		}

		public static void CreateDatabases(string name)
		{
			createOrRestoreCcc7(name);
			createOrRestoreAnalytics();
		}

		public static IDataSource CreateDataSource(ICurrentPersistCallbacks persistCallbacks)
		{
			return makeDataSource(persistCallbacks, TestTenantName);
		}

		public static IDataSource CreateDataSource(ICurrentPersistCallbacks persistCallbacks, string name)
		{
			return makeDataSource(persistCallbacks, name);
		}



		public static void BackupCcc7Database(int dataHash)
		{
			backupByFileCopy(ccc7(), dataHash);
		}

		public static void RestoreCcc7Database(int dataHash)
		{
			restoreByFileCopy(ccc7(), dataHash);
		}

		public static void BackupCcc7DatabaseBySql(string path, int dataHash)
		{
			var database = ccc7();
			database.BackupBySql().Backup(path, database.BackupName(dataHash));
		}

		public static bool TryRestoreCcc7DatabaseBySql(string path, int dataHash)
		{
			var database = ccc7();
			return database.BackupBySql().TryRestore(path, database.BackupName(dataHash));
		}



		public static void BackupAnalyticsDatabase(int dataHash)
		{
			backupByFileCopy(analytics(), dataHash);
		}

		public static void RestoreAnalyticsDatabase(int dataHash)
		{
			restoreByFileCopy(analytics(), dataHash);
		}

		public static void ClearAnalyticsData()
		{
			analytics().ConfigureSystem().CleanByAnalyticsProcedure();
		}

		public static void BackupAnalyticsDatabaseBySql(string path, int dataHash)
		{
			var database = analytics();
			database.BackupBySql().Backup(path, database.BackupName(dataHash));
		}

		public static bool TryRestoreAnalyticsDatabaseBySql(string path, int dataHash)
		{
			var database = analytics();
			return database.BackupBySql().TryRestore(path, database.BackupName(dataHash));
		}



		private static DatabaseHelper ccc7()
		{
			return new DatabaseHelper(InfraTestConfigReader.ConnectionString, DatabaseType.TeleoptiCCC7);
		}

		private static DatabaseHelper analytics()
		{
			return new DatabaseHelper(InfraTestConfigReader.AnalyticsConnectionString, DatabaseType.TeleoptiAnalytics);
		}

		private static void createOrRestoreCcc7(string name)
		{
			var database = ccc7();
			if (tryRestoreByFileCopy(database, 0))
			{
				database.ConfigureSystem().SetTenantConnectionInfo(name, database.ConnectionString, analytics().ConnectionString);
				return;
			}

			createDatabase(database);
			
			//would be better if dbmanager was called, but don't have the time right now....
			// eh, that thing that is called IS the db manager!
			ccc7().ConfigureSystem().MergePersonAssignments();
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

		private static void createDatabase(DatabaseHelper database)
		{
			database.CreateByDbManager();
			database.CreateSchemaByDbManager();
		}

		private static void backupByFileCopy(DatabaseHelper database, int dataHash)
		{
			var name = database.BackupName(dataHash);
			var backup = database.BackupByFileCopy().Backup(name);
			File.WriteAllText(name, JsonConvert.SerializeObject(backup, Formatting.Indented));
		}

		private static bool tryRestoreByFileCopy(DatabaseHelper database, int dataHash)
		{
			// maybe it would be possible to attach it if a file exists but the database doesnt. but wth..
			if (!database.Tasks().Exists(database.DatabaseName))
				return false;

			var name = database.BackupName(dataHash);
			if (!File.Exists(name))
				return false;

			var backup = JsonConvert.DeserializeObject<Backup>(File.ReadAllText(name));
			return database.BackupByFileCopy().TryRestore(backup);
		}

		private static void restoreByFileCopy(DatabaseHelper database, int dataHash)
		{
			var backup = JsonConvert.DeserializeObject<Backup>(File.ReadAllText(database.BackupName(dataHash)));
			var result = database.BackupByFileCopy().TryRestore(backup);
			if (!result)
				throw new Exception("Restore failed!");
		}
		

		




		private static IDataSource makeDataSource(ICurrentPersistCallbacks persistCallbacks, string name)
		{
			var dataSourceFactory = new DataSourcesFactory(new EnversConfiguration(), persistCallbacks, DataSourceConfigurationSetter.ForTest(), new CurrentHttpContext(), null);
			var dataSourceSettings = CreateDataSourceSettings(InfraTestConfigReader.ConnectionString, null, name);
			return dataSourceFactory.Create(dataSourceSettings, InfraTestConfigReader.AnalyticsConnectionString);
		}

		public static IDictionary<string, string> CreateDataSourceSettings(string connectionString, int? timeout, string sessionFactoryName)
		{
			var dictionary = new Dictionary<string, string>();
			if (sessionFactoryName != null)
				dictionary[Environment.SessionFactoryName] = sessionFactoryName;
			dictionary[Environment.Dialect] = typeof(MsSql2008Dialect).AssemblyQualifiedName;
			dictionary[Environment.ConnectionString] = connectionString;
			dictionary[Environment.SqlExceptionConverter] = typeof(SqlServerExceptionConverter).AssemblyQualifiedName;
			dictionary[Environment.CurrentSessionContextClass] = "call";
			if (timeout.HasValue)
				dictionary[Environment.CommandTimeout] = timeout.Value.ToString(CultureInfo.CurrentCulture);
			return dictionary;
		}
		
	}
}