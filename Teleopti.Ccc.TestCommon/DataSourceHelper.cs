using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.TestCommon
{
	public static class DataSourceHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static IDataSource CreateDataSource()
		{
			var dataSourceFactory = new DataSourcesFactory(new EnversConfiguration(), new List<IMessageSender>(), DataSourceConfigurationSetter.ForTest());

			using (var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7))
			{
				using (var analytics = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, DatabaseType.TeleoptiAnalytics))
				{

					if (IniFileInfo.Create)
						CreateDatabases(ccc7, analytics);

					var dataSource = CreateDataSource(dataSourceFactory);

					if (IniFileInfo.Create)
						CreateSchemas(dataSourceFactory, ccc7, analytics);

					BackupDatabases(ccc7, analytics);

					return dataSource;
				}
			}
		}

		private static void BackupDatabases(DatabaseHelper ccc7, DatabaseHelper analytics)
		{
			ExceptionToConsole(
				() =>
					{
						var ccc7BackupName = ccc7.DatabaseVersion() + "." + ccc7.SchemaTrunkHash();
						ccc7.BackupByFileCopy(ccc7BackupName);
					},
				"Failed to backup database {0}!", ConnectionStringHelper.ConnectionStringUsedInTests
				);

			ExceptionToConsole(
				() =>
					{
						var analyticsBackupName = analytics.DatabaseVersion() + "." + analytics.SchemaTrunkHash();
						analytics.BackupByFileCopy(analyticsBackupName);
					},
				"Failed to backup database {0}!", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix
				);
		}

		private static void CreateDatabases(DatabaseHelper ccc7, DatabaseHelper analytics)
		{
			ExceptionToConsole(
				() =>
					{
						if (ccc7.Exists())
						{
							ccc7.DropConnections();
							ccc7.Drop();
						}
						if (IniFileInfo.CreateByNHib)
							ccc7.Create();
						else
							ccc7.CreateByDbManager();
					},
				"Failed to prepare database {0}!", ConnectionStringHelper.ConnectionStringUsedInTests
				);

			ExceptionToConsole(
				() =>
					{
						if (analytics.Exists())
						{
							analytics.DropConnections();
							analytics.Drop();
						}
						analytics.CreateByDbManager();
					},
				"Failed to prepare database {0}!", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix
				);
		}

		private static IDataSource CreateDataSource(DataSourcesFactory dataSourceFactory)
		{
			return ExceptionToConsole(
				() =>
					{
						var dataSourceSettings = CreateDataSourceSettings(ConnectionStringHelper.ConnectionStringUsedInTests, null);
						return dataSourceFactory.Create(dataSourceSettings, ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
					},
				"Failed to create datasource {0}!", ConnectionStringHelper.ConnectionStringUsedInTests
				);
		}

		private static void CreateSchemas(DataSourcesFactory dataSourceFactory, DatabaseHelper ccc7, DatabaseHelper analytics)
		{
			ExceptionToConsole(
				() =>
					{
						if (IniFileInfo.CreateByNHib)
							dataSourceFactory.CreateSchema();
						else
							ccc7.CreateSchemaByDbManager();
						PersistAuditSetting();
					},
				"Failed to create database schema {0}!", ConnectionStringHelper.ConnectionStringUsedInTests
				);

			ExceptionToConsole(
				analytics.CreateSchemaByDbManager,
				"Failed to create database schema {0}!", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix
				);
		}


		public static void PersistAuditSetting()
		{
			using (var conn = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTests))
			{
				conn.Open();
				using(var cmd = new SqlCommand("delete from auditing.Auditsetting", conn))
					cmd.ExecuteNonQuery();
				using(var cmd = new SqlCommand("insert into auditing.Auditsetting (id, IsScheduleEnabled) values (" + AuditSetting.TheId + ",0)", conn))
					cmd.ExecuteNonQuery();
			}
		}



		public static IDictionary<string, string> CreateDataSourceSettings(string connectionString, int? timeout)
		{
			var dictionary = new Dictionary<string, string>();
			dictionary[NHibernate.Cfg.Environment.ConnectionProvider] =
				"Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TeleoptiDriverConnectionProvider, Teleopti.Ccc.Infrastructure";
			dictionary[NHibernate.Cfg.Environment.Dialect] = "NHibernate.Dialect.MsSql2005Dialect";
			dictionary[NHibernate.Cfg.Environment.ConnectionString] = connectionString;
			dictionary[NHibernate.Cfg.Environment.SqlExceptionConverter] = DataSourceSettingValues.SqlServerExceptionConverterTypeName;
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