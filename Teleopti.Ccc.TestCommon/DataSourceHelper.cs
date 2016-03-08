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
			setupCcc7(name);
			setupAnalytics();
			return makeDataSource(persistCallbacks, name);
		}
		
		public static void CreateDatabases()
		{
			CreateDatabases(TestTenantName);
		}

		public static void CreateDatabases(string name)
		{
			setupCcc7(name);
			setupAnalytics();
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
			backupDatabase(ccc7(), dataHash);
		}

		public static void RestoreCcc7Database(int dataHash)
		{
			restoreDatabase(ccc7(), dataHash);
		}

		public static void BackupAnalyticsDatabase(int dataHash)
		{
			backupDatabase(analytics(), dataHash);
		}

		public static void RestoreAnalyticsDatabase(int dataHash)
		{
			restoreDatabase(analytics(), dataHash);
		}

		public static void ClearAnalyticsData()
		{
			analytics().ConfigureSystem().CleanByAnalyticsProcedure();
		}






		private static DatabaseHelper ccc7()
		{
			return new DatabaseHelper(InfraTestConfigReader.ConnectionString, DatabaseType.TeleoptiCCC7);
		}

		private static DatabaseHelper analytics()
		{
			return new DatabaseHelper(InfraTestConfigReader.AnalyticsConnectionString, DatabaseType.TeleoptiAnalytics);
		}

		private static void setupCcc7(string name)
		{
			var database = ccc7();
			if (tryRestoreDatabase(database, 0))
			{
				database.ConfigureSystem().SetTenantConnectionInfo(name ?? String.Empty, database.ConnectionString, analytics().ConnectionString);
				return;
			}

			createDatabase(database);
			
			//would be better if dbmanager was called, but don't have the time right now....
			// eh, that thing that is called IS the db manager!
			ccc7().ConfigureSystem().MergePersonAssignments();
			database.ConfigureSystem().PersistAuditSetting();
			database.ConfigureSystem().SetTenantConnectionInfo(name ?? String.Empty, database.ConnectionString, analytics().ConnectionString);

			backupDatabase(database, 0);
		}

		private static void setupAnalytics()
		{
			var database = analytics();
			if (tryRestoreDatabase(database, 0))
				return;
			createDatabase(database);
			backupDatabase(database, 0);
		}

		private static void createDatabase(DatabaseHelper database)
		{
			database.CreateByDbManager();
			database.CreateSchemaByDbManager();
		}

		private static void backupDatabase(DatabaseHelper database, int dataHash)
		{
			var name = backupName(database.DatabaseType, database.DatabaseVersion(), database.OtherScriptFilesHash(), database.DatabaseName, dataHash);
			var backup = database.BackupByFileCopy().Backup(name);
			File.WriteAllText(name, JsonConvert.SerializeObject(backup, Formatting.Indented));
		}

		private static bool tryRestoreDatabase(DatabaseHelper database, int dataHash)
		{
			// maybe it would be possible to attach it if a file exists but the database doesnt. but wth..
			if (!database.Tasks().Exists(database.DatabaseName))
				return false;

			var name = backupName(database.DatabaseType, database.SchemaVersion(), database.OtherScriptFilesHash(), database.DatabaseName, dataHash);
			if (!File.Exists(name))
				return false;

			var backup = JsonConvert.DeserializeObject<Backup>(File.ReadAllText(name));
			return database.BackupByFileCopy().TryRestore(backup);
		}

		private static void restoreDatabase(DatabaseHelper database, int dataHash)
		{
			var name = backupName(database.DatabaseType, database.SchemaVersion(), database.OtherScriptFilesHash(), database.DatabaseName, dataHash);
			var backup = JsonConvert.DeserializeObject<Backup>(File.ReadAllText(name));
			var result = database.BackupByFileCopy().TryRestore(backup);
			if (!result)
				throw new Exception("Restore failed!");
		}

		private static string backupName(DatabaseType databaseType, int databaseVersion, int otherScriptFilesHash, string databaseName, int dataHash)
		{
			return databaseType + "." + databaseName + "." + databaseVersion + "." + otherScriptFilesHash + "." + dataHash + ".backup";
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