using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
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
			IDataSource dataSource;
			var dataSourceFactory = new DataSourcesFactory(new EnversConfiguration(),new List<IDenormalizer>()) { UseCache = false };

			try
			{
				if (IniFileInfo.Create)
				{
					DropDatabaseConnections();
					DropAndReCreateDatabase();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to recreate database! Remove tables manually from [" +
										ConnectionStringHelper.ConnectionStringUsedInTests + "] and/or make sure db exists.");
				Console.WriteLine(e.ToString());
				throw;
			}

			try
			{
				var dataSourceSettings = CreateDataSourceSettings(ConnectionStringHelper.ConnectionStringUsedInTests, null);
				dataSource = dataSourceFactory.Create(dataSourceSettings, ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
			}
			catch (Exception)
			{
				Console.WriteLine("Failed to create datasource for [" + ConnectionStringHelper.ConnectionStringUsedInTests + "].");
				throw;
			}

			try
			{
				if (IniFileInfo.Create)
				{
					dataSourceFactory.CreateSchema();
					PersistAuditSetting();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to recreate database! Remove tables manually from [" +
										ConnectionStringHelper.ConnectionStringUsedInTests + "] and/or make sure db exists.");
				Console.WriteLine(e.ToString());
				throw;
			}
			return dataSource;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public static void CleanDatabase()
		{
			using (var connection = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTests))
			{
				connection.Open();

				var command = connection.CreateCommand();
				command.CommandText = File.ReadAllText("Teleopti.Ccc.TestCommon.sp_deleteAllData.DROP.sql");
				command.ExecuteNonQuery();

				command = connection.CreateCommand();
				command.CommandText = File.ReadAllText("Teleopti.Ccc.TestCommon.sp_deleteAllData.sql");
				command.ExecuteNonQuery();

				command = connection.CreateCommand();
				command.CommandText = "EXEC sp_deleteAllData";
				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public static void DropDatabaseConnections()
		{
			using (var connection = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTests))
			{
				connection.Open();

				var command = connection.CreateCommand();
				command.CommandText = "USE master";
				command.ExecuteNonQuery();

				command = connection.CreateCommand();
				command.CommandText = string.Format("ALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE", IniFileInfo.Database);
				command.ExecuteNonQuery();

				command = connection.CreateCommand();
				command.CommandText = string.Format("ALTER DATABASE [{0}] SET ONLINE", IniFileInfo.Database);
				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public static void DropAndReCreateDatabase()
		{
			using (var connection = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTests))
			{
				connection.Open();

				var command = connection.CreateCommand();
				command.CommandText = "USE master";
				command.ExecuteNonQuery();

				command = connection.CreateCommand();
				command.CommandText = string.Format("DROP DATABASE [{0}]", IniFileInfo.Database);
				command.ExecuteNonQuery();

				command = connection.CreateCommand();
				command.CommandText = string.Format("CREATE DATABASE [{0}]", IniFileInfo.Database);
				command.ExecuteNonQuery();
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