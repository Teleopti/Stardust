using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.TestCommon
{
	public static class DataSourceHelper
	{
		public static IDataSource CreateDataSource()
		{
			var ccc7 = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTests, DatabaseType.TeleoptiCCC7);
			var analytics = new DatabaseHelper(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, DatabaseType.TeleoptiAnalytics);

			if (IniFileInfo.Create)
				PrepareDatabases(ccc7, analytics);

			var dataSourceFactory = new DataSourcesFactory(new EnversConfiguration(), new List<IDenormalizer>()) { UseCache = false };
			var dataSource = CreateDataSource(dataSourceFactory);

			if (IniFileInfo.Create)
				CreateSchemas(dataSourceFactory, ccc7, analytics);

			return dataSource;
		}

		private static void PrepareDatabases(DatabaseHelper ccc7, DatabaseHelper analytics)
		{
			try
			{
				ccc7.DropConnections();
				ccc7.Drop();
				ccc7.Create();
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to prepare database {0}!", ConnectionStringHelper.ConnectionStringUsedInTests);
				Console.WriteLine(e.ToString());
				throw;
			}

			try
			{
				analytics.DropConnections();
				analytics.Drop();
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
				dataSourceFactory.CreateSchema();
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
			dictionary[DataSourceSettings.ConnectionProvider] =
				"Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TeleoptiDriverConnectionProvider, Teleopti.Ccc.Infrastructure";
			dictionary[DataSourceSettings.Dialect] = "NHibernate.Dialect.MsSql2005Dialect";
			dictionary[DataSourceSettings.ConnectionString] = connectionString;
			dictionary[DataSourceSettings.SqlExceptionConverter] = DataSourceSettingValues.SqlServerExceptionConverterTypeName;
			dictionary[DataSourceSettings.CurrentSessionContextClass] = "call";
			if (timeout.HasValue)
				dictionary[DataSourceSettings.CommandTimeout] = timeout.Value.ToString(CultureInfo.CurrentCulture);
			return dictionary;
		}
	}
}