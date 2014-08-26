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
		public static IDataSource CreateDataSource(IEnumerable<IMessageSender> messageSenders, string name)
		{
			var dataSourceFactory = new DataSourcesFactory(new EnversConfiguration(), messageSenders, DataSourceConfigurationSetter.ForTest());
			var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7);
			if (!TryRestoreDatabase(ccc7))
			{
				createCcc7(ccc7);
				BackupDatabase(ccc7, 0);
			}
			SetupAnalytics();
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
			SetupAnalytics();
			return CreateDataSource(dataSourceFactory, "TestData");
		}

		private static void createCcc7(DatabaseHelper ccc7)
		{
			createDatabase(ccc7);
			CreateUniqueIndexOnPersonAssignmentBecauseDbManagerIsntRunFromTests();
			PersistAuditSetting();
		}

		private static void CreateUniqueIndexOnPersonAssignmentBecauseDbManagerIsntRunFromTests()
		{
			//would be better if dbmanager was called, but don't have the time right now....
			ExceptionToConsole(
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

		private static void SetupAnalytics()
		{
			var analytics = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, DatabaseType.TeleoptiAnalytics);
			if (TryRestoreDatabase(analytics))
				return;

			createDatabase(analytics);
			BackupDatabase(analytics, 0);
		}

		private static void createDatabase(DatabaseHelper database)
		{
			ExceptionToConsole(
				database.CreateByDbManager,
				"Failed to prepare database {0}!", database.ConnectionString
				);
			ExceptionToConsole(
				database.CreateSchemaByDbManager,
				"Failed to create database schema {0}!", database.ConnectionString
				);
		}

		private static bool TryRestoreDatabase(DatabaseHelper database)
		{
			return ExceptionToConsole(
				() =>
				{
					// maybe it would be possible to attach it if a file exists but the database doesnt. but wth..
					if (!database.Exists())
						return false;

					var backupName = BackupName(database.DatabaseType, database.SchemaVersion(), database.OtherScriptFilesHash(), database.DatabaseName, 0);
					var fileName = backupName + ".backup";
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
			var backupName = BackupName(ccc7.DatabaseType, ccc7.SchemaVersion(), ccc7.OtherScriptFilesHash(), ccc7.DatabaseName, dataHash);
			var fileName = backupName + ".backup";
			var backup = JsonConvert.DeserializeObject<DatabaseHelper.Backup>(File.ReadAllText(fileName));
			if(beforeRestore!=null)
				beforeRestore();
			ccc7.RestoreByFileCopy(backup);
		}

		public static void BackupCcc7Database(int dataHash)
		{
			var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7);
			BackupDatabase(ccc7, dataHash);
		}

		public static bool Ccc7BackupExists(int dataHash)
		{
			var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7);
			var backupName = BackupName(ccc7.DatabaseType, ccc7.SchemaVersion(), ccc7.OtherScriptFilesHash(), ccc7.DatabaseName, dataHash);
			var fileName = backupName + ".backup";
			return File.Exists(fileName);
		}

		public static void BackupDatabase(DatabaseHelper database, int dataHash)
		{
			ExceptionToConsole(
				() =>
				{
					var backupName = BackupName(database.DatabaseType, database.DatabaseVersion(), database.OtherScriptFilesHash(), database.DatabaseName, dataHash);
					var backup = database.BackupByFileCopy(backupName);
					var fileName = backupName + ".backup";
					File.WriteAllText(fileName, JsonConvert.SerializeObject(backup, Formatting.Indented));
				},
				"Failed to backup database {0}!", database.ConnectionString
				);
		}

		private static string BackupName(DatabaseType databaseType, int databaseVersion, int otherScriptFilesHash, string databaseName, int dataHash)
		{
			return databaseType + "." + databaseName + "." + databaseVersion + "." + otherScriptFilesHash + "." + dataHash;
		}

		private static IDataSource CreateDataSource(DataSourcesFactory dataSourceFactory, string name)
		{
			return ExceptionToConsole(
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
			ExceptionToConsole(
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

		private static void ExceptionToConsole(Action action, string exceptionMessage, params object[] args)
		{
			try
			{
				action.Invoke();
			}
			catch (Exception e)
			{
				Console.WriteLine(string.Format(exceptionMessage, args));
				Console.WriteLine(e.ToString());
				throw;
			}
		}

		private static T ExceptionToConsole<T>(Func<T> action, string exceptionMessage, params object[] args)
		{
			try
			{
				return action.Invoke();
			}
			catch (Exception e)
			{
				Console.WriteLine(string.Format(exceptionMessage, args));
				Console.WriteLine(e.ToString());
				throw;
			}
		}
	}
}