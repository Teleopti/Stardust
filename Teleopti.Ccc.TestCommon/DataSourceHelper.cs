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
			using (var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7))
			{
				using (var analytics = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, DatabaseType.TeleoptiAnalytics))
				{
					if (IniFileInfo.Create)
						PrepareDatabases(ccc7, analytics);

					var dataSourceFactory = new DataSourcesFactory(new EnversConfiguration(), new List<IMessageSender>(), DataSourceConfigurationSetter.ForTest());
					var dataSource = CreateDataSource(dataSourceFactory);

					if (IniFileInfo.Create)
						CreateSchemas(dataSourceFactory, ccc7, analytics);

					return dataSource;
				}
			}
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

		public static DatabaseHelper.Backup BackupCcc7DataByFileCopy()
		{
			using (var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7))
			{
				var result = ccc7.BackupByFileCopy();
				ccc7.ClearPool();
				Thread.Sleep(1000);
				return result;
			}
		}

		public static void RestoreCcc7DataByFileCopy(DatabaseHelper.Backup backup)
		{
			using (var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7))
			{
				ccc7.RestoreByFileCopy(backup);
				ccc7.ClearPool();
				Thread.Sleep(1000);
			}
		}

		private static void PrepareDatabases(DatabaseHelper ccc7, DatabaseHelper analytics)
		{
			try
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
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to prepare database {0}!", ConnectionStringHelper.ConnectionStringUsedInTests);
				Console.WriteLine(e.ToString());
				throw;
			}

			try
			{
				if (analytics.Exists())
				{
					analytics.DropConnections();
					analytics.Drop();
				}
				analytics.CreateByDbManager();
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to prepare database {0}!", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
				Console.WriteLine(e.ToString());
				throw;
			}
		}

		private static IDataSource CreateDataSource(DataSourcesFactory dataSourceFactory)
		{
			IDataSource dataSource;
			try
			{
				var dataSourceSettings = CreateDataSourceSettings(ConnectionStringHelper.ConnectionStringUsedInTests, null);
				dataSource = dataSourceFactory.Create(dataSourceSettings, ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
			}
			catch (Exception)
			{
				Console.WriteLine("Failed to create datasource {0}!", ConnectionStringHelper.ConnectionStringUsedInTests);
				throw;
			}
			return dataSource;
		}

		private static void CreateSchemas(DataSourcesFactory dataSourceFactory, DatabaseHelper ccc7, DatabaseHelper analytics)
		{
			try
			{
				if (IniFileInfo.CreateByNHib)
					dataSourceFactory.CreateSchema();
				else
					ccc7.CreateSchemaByDbManager();
				PersistAuditSetting();
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to create database schema {0}!", ConnectionStringHelper.ConnectionStringUsedInTests);
				Console.WriteLine(e.ToString());
				throw;
			}

			try
			{
				analytics.CreateSchemaByDbManager();
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to create database schema {0}!", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
				Console.WriteLine(e.ToString());
				throw;
			}
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
	}
}