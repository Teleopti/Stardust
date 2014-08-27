using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.TestCommon
{
	public static class DataSourceHelper
	{
		private const string backupFileDbWithoutDataFileExtension = ".backup";
		private const string backupFileDbWithDataFileExtension = ".backupWithData";

		public static IDataSource CreateDataSource(IEnumerable<IMessageSender> messageSenders, string name)
		{
			var dataSourceFactory = new DataSourcesFactory(new EnversConfiguration(), messageSenders, DataSourceConfigurationSetter.ForTest());
			var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7);
			if (!tryRestoreDatabase(ccc7))
			{
				createCcc7(ccc7);
				backupDatabase(ccc7, 0, backupFileDbWithoutDataFileExtension);
			}
			setupAnalytics();
			return CreateDataSource(dataSourceFactory, name);
		}

		public static IDataSource CreateDataSourceNoBackup(IEnumerable<IMessageSender> messageSenders, bool createCcc7Database)
		{
			var dataSourceFactory = new DataSourcesFactory(new EnversConfiguration(), messageSenders, DataSourceConfigurationSetter.ForTest());
			if (createCcc7Database)
			{
				var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7);
				createCcc7(ccc7);
			}
			setupAnalytics();
			return CreateDataSource(dataSourceFactory, "TestData");
		}

		private static void createCcc7(DatabaseHelper ccc7)
		{
			createDatabase(ccc7);
			createUniqueIndexOnPersonAssignmentBecauseDbManagerIsntRunFromTests();
			PersistAuditSetting();
		}

		private static void createUniqueIndexOnPersonAssignmentBecauseDbManagerIsntRunFromTests()
		{
			//would be better if dbmanager was called, but don't have the time right now....
			exceptionToConsole(
				() =>
				{
					using (var conn = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTests))
					{
						SqlConnection.ClearPool(conn);
						conn.Open();
						using (var cmd = new SqlCommand("exec [dbo].[MergePersonAssignments]", conn))
							cmd.ExecuteNonQuery();
					}
				},
				"Failed to create unique index on personassignment in database {0}!", ConnectionStringHelper.ConnectionStringUsedInTests
				);
		}

		private static void setupAnalytics()
		{
			var analytics = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, DatabaseType.TeleoptiAnalytics);
			if (tryRestoreDatabase(analytics))
				return;

			createDatabase(analytics);
			backupDatabase(analytics, 0, backupFileDbWithoutDataFileExtension);
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

		private static bool tryRestoreDatabase(DatabaseHelper database)
		{
			return exceptionToConsole(
				() =>
				{
					// maybe it would be possible to attach it if a file exists but the database doesnt. but wth..
					if (!database.Exists())
						return false;

					var backupName = DataSourceHelper.backupName(database.DatabaseType, database.SchemaVersion(), database.OtherScriptFilesHash(), database.DatabaseName, 0);
					var fileName = backupName + backupFileDbWithoutDataFileExtension;
					if (!File.Exists(fileName))
						return false;

					var backup = JsonConvert.DeserializeObject<DatabaseHelper.Backup>(File.ReadAllText(fileName));
					database.RestoreByFileCopy(backup);
					return true;
				},
				"Failed to backup database {0}!", database.ConnectionString
				);
		}

		public static void RestoreCcc7Database(int dataHash, Action beforeRestore)
		{
			var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7);
			var backupName = DataSourceHelper.backupName(ccc7.DatabaseType, ccc7.SchemaVersion(), ccc7.OtherScriptFilesHash(), ccc7.DatabaseName, dataHash);
			var fileName = backupName + backupFileDbWithDataFileExtension;
			var backup = JsonConvert.DeserializeObject<DatabaseHelper.Backup>(File.ReadAllText(fileName));
			if(beforeRestore!=null)
				beforeRestore();
			ccc7.RestoreByFileCopy(backup);
		}

		public static void BackupCcc7Database(int dataHash)
		{
			var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7);
			backupDatabase(ccc7, dataHash, backupFileDbWithDataFileExtension);
		}

		public static bool Ccc7BackupExists(int dataHash)
		{
			var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7);
			var backupName = DataSourceHelper.backupName(ccc7.DatabaseType, ccc7.SchemaVersion(), ccc7.OtherScriptFilesHash(), ccc7.DatabaseName, dataHash);
			var fileName = backupName + backupFileDbWithDataFileExtension;
			return File.Exists(fileName);
		}

		private static void backupDatabase(DatabaseHelper database, int dataHash, string fileExtension)
		{
			exceptionToConsole(
				() =>
				{
					var backupName = DataSourceHelper.backupName(database.DatabaseType, database.DatabaseVersion(), database.OtherScriptFilesHash(), database.DatabaseName, dataHash);
					var backup = database.BackupByFileCopy(backupName);
					var fileName = backupName + fileExtension;
					File.WriteAllText(fileName, JsonConvert.SerializeObject(backup, Formatting.Indented));
				},
				"Failed to backup database {0}!", database.ConnectionString
				);
		}

		private static string backupName(DatabaseType databaseType, int databaseVersion, int otherScriptFilesHash, string databaseName, int dataHash)
		{
			return databaseType + "." + databaseName + "." + databaseVersion + "." + otherScriptFilesHash + "." + dataHash;
		}

		private static IDataSource CreateDataSource(DataSourcesFactory dataSourceFactory, string name)
		{
			return exceptionToConsole(
				() =>
				{
					var dataSourceSettings = CreateDataSourceSettings(ConnectionStringHelper.ConnectionStringUsedInTests, null, name);
					return dataSourceFactory.Create(dataSourceSettings, ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
				},
				"Failed to create datasource {0}!", ConnectionStringHelper.ConnectionStringUsedInTests
				);
		}

		public static void PersistAuditSetting()
		{
			exceptionToConsole(
				() =>
				{
					using (var conn = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTests))
					{
						SqlConnection.ClearPool(conn);
						conn.Open();
						using (var cmd = new SqlCommand("delete from auditing.Auditsetting", conn))
							cmd.ExecuteNonQuery();
						using (var cmd = new SqlCommand("insert into auditing.Auditsetting (id, IsScheduleEnabled) values (" + AuditSetting.TheId + ",0)", conn))
							cmd.ExecuteNonQuery();
					}
				},
				"Failed to persistn audit setting in database {0}!", ConnectionStringHelper.ConnectionStringUsedInTests
				);
		}

		public static IDictionary<string, string> CreateDataSourceSettings(string connectionString, int? timeout, string sessionFactoryName)
		{
			var dictionary = new Dictionary<string, string>();
			if (sessionFactoryName != null)
				dictionary[NHibernate.Cfg.Environment.SessionFactoryName] = sessionFactoryName;
			dictionary[NHibernate.Cfg.Environment.ConnectionProvider] =
				"Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TeleoptiDriverConnectionProvider, Teleopti.Ccc.Infrastructure";
			dictionary[NHibernate.Cfg.Environment.Dialect] = "NHibernate.Dialect.MsSql2005Dialect";
			dictionary[NHibernate.Cfg.Environment.ConnectionString] = connectionString;
			dictionary[NHibernate.Cfg.Environment.SqlExceptionConverter] = typeof(SqlServerExceptionConverter).AssemblyQualifiedName;
			dictionary[NHibernate.Cfg.Environment.CurrentSessionContextClass] = "call";
			if (timeout.HasValue)
				dictionary[NHibernate.Cfg.Environment.CommandTimeout] = timeout.Value.ToString(CultureInfo.CurrentCulture);
			return dictionary;
		}

		public static void ClearAnalyticsData()
		{
			var analytics = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, DatabaseType.TeleoptiAnalytics);
			analytics.CleanByAnalyticsProcedure();
		}

		public static DatabaseHelper.Backup BackupCcc7DataByFileCopy(string name)
		{
			var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7);
			return ccc7.BackupByFileCopy(name);
		}

		public static void RestoreCcc7DataByFileCopy(DatabaseHelper.Backup backup)
		{
			var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7);
			ccc7.RestoreByFileCopy(backup);
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
	}
}