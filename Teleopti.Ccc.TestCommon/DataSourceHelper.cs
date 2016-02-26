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

namespace Teleopti.Ccc.TestCommon
{
	public static class DataSourceHelper
	{
		public static IDataSource CreateDatabasesAndDataSource(ICurrentPersistCallbacks persistCallbacks, string name)
		{
			setupCcc7(name);
			setupAnalytics();
			return makeDataSource(persistCallbacks, name);
		}

		public static void CreateDatabases(string name)
		{
			setupCcc7(name);
			setupAnalytics();
		}

		public static IDataSource CreateDataSource(ICurrentPersistCallbacks persistCallbacks, string name)
		{
			return makeDataSource(persistCallbacks, name);
		}

		public static void RestoreCcc7Database(int dataHash)
		{
			var databaseHelper = ccc7();
			restoreDatabase(databaseHelper, databaseHelper.BackupHelper(), dataHash);
		}

		public static void BackupCcc7Database(int dataHash)
		{
			var databaseHelper = ccc7();
			backupDatabase(databaseHelper, databaseHelper.BackupHelper(), dataHash);
		}

		public static void BackupAnalyticsDatabase(int dataHash)
		{
			var databaseHelper = analytics();
			backupDatabase(databaseHelper, databaseHelper.BackupHelper(), dataHash);
		}

		public static void RestoreAnalyticsDatabase(int dataHash)
		{
			var databaseHelper = analytics();
			restoreDatabase(databaseHelper, databaseHelper.BackupHelper(), dataHash);
		}

		public static void ClearAnalyticsData()
		{
			analytics().ConfigureSystem().CleanByAnalyticsProcedure();
		}

		public static void PersistAuditSetting()
		{
			exceptionToConsole(
				() =>
				{
					var databaseHelper = ccc7();
					databaseHelper.ConfigureSystem().PersistAuditSetting();
				},
				"Failed to persist audit setting in database {0}!", ConnectionStringHelper.ConnectionStringUsedInTests
				);
		}

		public static IDictionary<string, string> CreateDataSourceSettings(string connectionString, int? timeout, string sessionFactoryName)
		{
			var dictionary = new Dictionary<string, string>();
			if (sessionFactoryName != null)
				dictionary[NHibernate.Cfg.Environment.SessionFactoryName] = sessionFactoryName;
			dictionary[NHibernate.Cfg.Environment.Dialect] = typeof(MsSql2008Dialect).AssemblyQualifiedName;
			dictionary[NHibernate.Cfg.Environment.ConnectionString] = connectionString;
			dictionary[NHibernate.Cfg.Environment.SqlExceptionConverter] = typeof(SqlServerExceptionConverter).AssemblyQualifiedName;
			dictionary[NHibernate.Cfg.Environment.CurrentSessionContextClass] = "call";
			if (timeout.HasValue)
				dictionary[NHibernate.Cfg.Environment.CommandTimeout] = timeout.Value.ToString(CultureInfo.CurrentCulture);
			return dictionary;
		}

		private static void setupCcc7(string name)
		{
			var databaseHelper = ccc7();
			var backupHelper = databaseHelper.BackupHelper();
			if (tryRestoreDatabase(databaseHelper, backupHelper, 0))
			{
				update_default_tenant_because_connstrings_arent_set_due_to_securityexe_isnt_run_from_infra_test_projs(name);
				return;
			}
			createDatabase(databaseHelper);
			createUniqueIndexOnPersonAssignmentBecauseDbManagerIsntRunFromTests();
			PersistAuditSetting();
			update_default_tenant_because_connstrings_arent_set_due_to_securityexe_isnt_run_from_infra_test_projs(name);
			backupDatabase(databaseHelper, backupHelper, 0);
		}

		private static void update_default_tenant_because_connstrings_arent_set_due_to_securityexe_isnt_run_from_infra_test_projs(string name)
		{
			var databaseHelper = ccc7();
			databaseHelper.ConfigureSystem().SetTenantConnectionInfo(name ?? string.Empty, databaseHelper.ConnectionString, analytics().ConnectionString);
		}

		private static void setupAnalytics()
		{
			var databaseHelper = analytics();
			var backupHelper = databaseHelper.BackupHelper();
			if (tryRestoreDatabase(databaseHelper, backupHelper, 0))
				return;
			createDatabase(databaseHelper);
			backupDatabase(databaseHelper, backupHelper, 0);
		}

		private static void createDatabase(DatabaseHelper database)
		{
			exceptionToConsole(
				database.CreateByDbManager,
				"Failed to prepare database {0}!", database.ConnectionString
				);
			exceptionToConsole(
				database.CreateSchemaByDbManager,
				"Failed to create database schema {0}!", database.ConnectionString
				);
		}

		private static void backupDatabase(DatabaseHelper database, BackupHelper backupHelper, int dataHash)
		{
			exceptionToConsole(
				() =>
				{
					var name = backupName(database.DatabaseType, database.DatabaseVersion(), database.OtherScriptFilesHash(), database.DatabaseName, dataHash);
					var backup = backupHelper.BackupByFileCopy(name);
					File.WriteAllText(name, JsonConvert.SerializeObject(backup, Formatting.Indented));
				},
				"Failed to backup database {0}!", database.ConnectionString
				);
		}

		private static bool tryRestoreDatabase(DatabaseHelper database, BackupHelper backupHelper, int dataHash)
		{
			return exceptionToConsole(
				() =>
				{
					// maybe it would be possible to attach it if a file exists but the database doesnt. but wth..
					if (!database.Tasks().Exists(database.DatabaseName))
						return false;

					var name = backupName(database.DatabaseType, database.SchemaVersion(), database.OtherScriptFilesHash(), database.DatabaseName, dataHash);
					if (!File.Exists(name))
						return false;

					var backup = JsonConvert.DeserializeObject<BackupHelper.Backup>(File.ReadAllText(name));
					return backupHelper.TryRestoreByFileCopy(backup);
				},
				"Failed to restore database {0}!", database.ConnectionString
				);
		}

		private static void restoreDatabase(DatabaseHelper database, BackupHelper backupHelper, int dataHash)
		{
			exceptionToConsole(
				() =>
				{
					var name = backupName(database.DatabaseType, database.SchemaVersion(), database.OtherScriptFilesHash(), database.DatabaseName, dataHash);
					var backup = JsonConvert.DeserializeObject<BackupHelper.Backup>(File.ReadAllText(name));
					var result = backupHelper.TryRestoreByFileCopy(backup);
					if (!result)
						throw new Exception("Restore failed!");
				},
				"Failed to restore database {0}!", database.ConnectionString
				);
		}

		private static string backupName(DatabaseType databaseType, int databaseVersion, int otherScriptFilesHash, string databaseName, int dataHash)
		{
			return databaseType + "." + databaseName + "." + databaseVersion + "." + otherScriptFilesHash + "." + dataHash + ".backup";
		}

		private static DatabaseHelper ccc7()
		{
			return new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7);
		}

		private static DatabaseHelper analytics()
		{
			return new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, DatabaseType.TeleoptiAnalytics);
		}

		private static IDataSource makeDataSource(ICurrentPersistCallbacks persistCallbacks, string name)
		{
			return exceptionToConsole(
				() =>
				{
					var dataSourceFactory = new DataSourcesFactory(new EnversConfiguration(), persistCallbacks, DataSourceConfigurationSetter.ForTest(), new CurrentHttpContext(), null);
					var dataSourceSettings = CreateDataSourceSettings(ConnectionStringHelper.ConnectionStringUsedInTests, null, name);
					return dataSourceFactory.Create(dataSourceSettings, ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
				},
				"Failed to create datasource {0}!", ConnectionStringHelper.ConnectionStringUsedInTests
				);
		}

		private static void exceptionToConsole(Action action, string exceptionMessage, params object[] args)
		{
			try
			{
				action.Invoke();
			}
			catch (Exception e)
			{
				Console.WriteLine(exceptionMessage, args);
				Console.WriteLine(e.ToString());
				throw;
			}
		}

		private static T exceptionToConsole<T>(Func<T> action, string exceptionMessage, params object[] args)
		{
			try
			{
				return action.Invoke();
			}
			catch (Exception e)
			{
				Console.WriteLine(exceptionMessage, args);
				Console.WriteLine(e.ToString());
				throw;
			}
		}

		private static void createUniqueIndexOnPersonAssignmentBecauseDbManagerIsntRunFromTests()
		{
			//would be better if dbmanager was called, but don't have the time right now....
			exceptionToConsole(
				() => ccc7().ConfigureSystem().MergePersonAssignments(),
				"Failed to create unique index on personassignment in database {0}!", ConnectionStringHelper.ConnectionStringUsedInTests
				);
		}
	}
}