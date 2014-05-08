using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
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

			using (var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7))
				SetupCcc7(ccc7);

			using (var analytics = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, DatabaseType.TeleoptiAnalytics))
				SetupAnalytics(analytics);

			return CreateDataSource(dataSourceFactory, name);
		}

		private static void SetupCcc7(DatabaseHelper ccc7)
		{
			if (TryRestoreDatabase(ccc7))
				return;

			CreateDatabase(ccc7);
			CreateSchema(ccc7);
			CreateUniqueIndexOnPersonAssignmentBecauseDbManagerIsntRunFromTests();
			PersistAuditSetting();
			BackupDatabase(ccc7);
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

		private static void SetupAnalytics(DatabaseHelper analytics)
		{
			if (TryRestoreDatabase(analytics))
				return;

			CreateDatabase(analytics);
			CreateSchema(analytics);
			BackupDatabase(analytics);
		}

		private static void CreateDatabase(DatabaseHelper database)
		{
			ExceptionToConsole(
				() =>
				{
					if (database.Exists())
					{
						database.DropConnections();
						database.Drop();
					}
					database.CreateByDbManager();
				},
				"Failed to prepare database {0}!", database.ConnectionString
				);
		}

		private static void CreateSchema(DatabaseHelper database)
		{
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

					var backupName = BackupName(database.DatabaseType, database.SchemaVersion(), database.OtherScriptFilesHash(), database.DatabaseName);
					var fileName = backupName + ".backup";
					if (!System.IO.File.Exists(fileName))
						return false;

					var backup = JsonConvert.DeserializeObject<DatabaseHelper.Backup>(System.IO.File.ReadAllText(fileName));
					database.RestoreByFileCopy(backup);
					return true;
				},
				"Failed to backup database {0}!", database.ConnectionString
				);

		}

		private static void BackupDatabase(DatabaseHelper database)
		{
			ExceptionToConsole(
				() =>
				{
					var backupName = BackupName(database.DatabaseType, database.DatabaseVersion(), database.OtherScriptFilesHash(), database.DatabaseName);
					var backup = database.BackupByFileCopy(backupName);
					var fileName = backupName + ".backup";
					System.IO.File.WriteAllText(fileName, JsonConvert.SerializeObject(backup, Formatting.Indented));
				},
				"Failed to backup database {0}!", database.ConnectionString
				);
		}

		private static string BackupName(DatabaseType databaseType, int databaseVersion, int otherScriptFilesHash, string databaseName)
		{
			return databaseType + "." + databaseName + "." + databaseVersion + "." + otherScriptFilesHash;
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

		public static void ClearCcc7Data()
		{
			using (var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7))
			{
				ccc7.CleanByGenericProcedure();
			}
		}

		public static void ClearAnalyticsData()
		{
			using (var analytics = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, DatabaseType.TeleoptiAnalytics))
			{
				analytics.CleanByAnalyticsProcedure();
			}
		}

		public static DatabaseHelper.Backup BackupCcc7DataByFileCopy(string name)
		{
			using (var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7))
			{
				return ccc7.BackupByFileCopy(name);
			}
		}

		public static void RestoreCcc7DataByFileCopy(DatabaseHelper.Backup backup)
		{
			using (var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7))
			{
				ccc7.RestoreByFileCopy(backup);
			}
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