using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.TestCommon
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class DatabaseHelper : IDisposable
	{
		private SqlConnection _connection;

		public DatabaseHelper(string connectionString, DatabaseType databaseType) : this(connectionString, new SqlConnectionStringBuilder(connectionString).InitialCatalog, databaseType) { }
		public DatabaseHelper(string connectionString, string databaseName, DatabaseType databaseType)
		{
			ConnectionString = connectionString;
			DatabaseName = databaseName;
			DatabaseType = databaseType;
		}

		public string ConnectionString { get; private set; }
		public DatabaseType DatabaseType { get; private set; }
		public string DatabaseName { get; private set; }

		private SqlConnection Connection()
		{
			if (_connection == null)
			{
				var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
				connectionStringBuilder.InitialCatalog = "master";
				_connection = new SqlConnection(connectionStringBuilder.ConnectionString);
				_connection.Open();
				if (Exists())
					ExecuteNonQuery("USE [{0}]", DatabaseName);
			}
			return _connection;
		}

		public bool Exists()
		{
			var databaseId = ExecuteScalar<int>("SELECT database_id FROM sys.databases WHERE Name = '{0}'", 0, DatabaseName);
			return databaseId > 0;
		}

		public void Drop()
		{
			ExecuteNonQuery("USE master");
			ExecuteNonQuery("DROP DATABASE [{0}]", DatabaseName);
		}

		public void Create()
		{
			ExecuteNonQuery("CREATE DATABASE [{0}]", DatabaseName);
			ExecuteNonQuery("USE [{0}]", DatabaseName);
		}

		public void CreateByDbManager()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder());
			var creator = new DatabaseCreator(databaseFolder, _connection);
			creator.CreateDatabase(DatabaseType, DatabaseName);
			ExecuteNonQuery("USE [{0}]", DatabaseName);
		}

		public void CleanByGenericProcedure()
		{
			ExecuteNonQuery(File.ReadAllText("Teleopti.Ccc.TestCommon.sp_deleteAllData.DROP.sql"));
			ExecuteNonQuery(File.ReadAllText("Teleopti.Ccc.TestCommon.sp_deleteAllData.sql"));
			ExecuteNonQuery("EXEC sp_deleteAllData");
		}

		public void CleanByAnalyticsProcedure()
		{
			ExecuteNonQuery("EXEC [mart].[etl_data_mart_delete] @DeleteAll=1");
		}

		public void DropConnections()
		{
			using (OfflineScope())
			{
			}
		}

		public void CreateSchemaByDbManager()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder());
			var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder, _connection);
			databaseVersionInformation.CreateTable();
			var schemaVersionInformation = new SchemaVersionInformation(databaseFolder);
			var schemaCreator = new DatabaseSchemaCreator(
				databaseVersionInformation,
				schemaVersionInformation,
				_connection,
				databaseFolder,
				new NullLog());
			schemaCreator.Create(DatabaseType);
		}

		public Backup BackupByFileCopy(string name)
		{
			var backup = new Backup();
			using (var reader = ExecuteToReader("select * from sys.sysfiles"))
			{
				backup.Files = (from r in reader.Cast<IDataRecord>()
								let file = r.GetString(r.GetOrdinal("filename"))
								select new BackupFile { Source = file })
					.ToArray();
			}
			using (OfflineScope())
			{
				backup.Files.ForEach(f =>
				                     	{
				                     		f.Backup = Path.GetFileName(f.Source) + "." + name;
				                     		File.Copy(f.Source, f.Backup, true);
				                     	});
			}
			return backup;
		}

		public void RestoreByFileCopy(Backup backup)
		{
			using (OfflineScope())
			{
				backup.Files.ForEach(f => File.Copy(f.Backup, f.Source, true));
			}
		}

		public class Backup
		{
			public IEnumerable<BackupFile> Files { get; set; }
		}

		public class BackupFile
		{
			public string Source { get; set; }
			public string Backup { get; set; }
		}



		public int DatabaseVersion()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder());
			var versionInfo = new DatabaseVersionInformation(databaseFolder, _connection);
			return versionInfo.GetDatabaseVersion();
		}

		public int SchemaVersion()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder());
			var versionInfo = new SchemaVersionInformation(databaseFolder);
			return versionInfo.GetSchemaVersion(DatabaseType);
		}

		public int OtherScriptFilesHash()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder());
			var versionInfo = new SchemaVersionInformation(databaseFolder);
			return versionInfo.GetOtherScriptFilesHash(DatabaseType);
		}




		private IDisposable OfflineScope()
		{
			ExecuteNonQuery("USE master");
			killSpidsTheHardWayBecauseItIsALotQuickerThan_SET_OFFLINE_WITH_IMMEDIATE_ROLLBACK();
			ExecuteNonQuery("ALTER DATABASE [{0}] SET OFFLINE", DatabaseName);
			return new GenericDisposable(() =>
			                             	{
			                             		ExecuteNonQuery("ALTER DATABASE [{0}] SET ONLINE", DatabaseName);
			                             		ExecuteNonQuery("USE [{0}]", DatabaseName);
			                             	});
		}

		private void killSpidsTheHardWayBecauseItIsALotQuickerThan_SET_OFFLINE_WITH_IMMEDIATE_ROLLBACK()
		{
			const string killCmd = @"DECLARE @dbname sysname
SET @dbname = '{0}'

DECLARE @spid int
SELECT @spid = min(session_id) from sys.dm_exec_sessions where database_id = db_id(@dbname) and is_user_process=1
WHILE @spid IS NOT NULL
BEGIN
EXECUTE ('KILL ' + @spid)
SELECT @spid = min(session_id) from sys.dm_exec_sessions where database_id = db_id(@dbname) AND session_id > @spid and is_user_process=1
END";
			ExecuteNonQuery(killCmd, DatabaseName);
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private void ExecuteNonQuery(string sql, params object[] args)
		{
			using (var command = Connection().CreateCommand())
			{
				command.CommandText = string.Format(sql, args);
				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private T ExecuteScalar<T>(string sql, T nullValue, params object[] args)
		{
			using (var command = Connection().CreateCommand())
			{
				command.CommandText = string.Format(sql, args);
				var value = command.ExecuteScalar();
				if (value == null)
					return nullValue;
				return (T) value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private SqlDataReader ExecuteToReader(string sql, params object[] args)
		{
			using (var command = Connection().CreateCommand())
			{
				command.CommandText = string.Format(sql, args);
				return command.ExecuteReader();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		public void Dispose()
		{
			if (_connection != null)
				_connection.Dispose();
		}
	}
}