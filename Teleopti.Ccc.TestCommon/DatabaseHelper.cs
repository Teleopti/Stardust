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
	public class DatabaseHelper
	{
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
			var conn = new SqlConnection(ConnectionString);
			conn.Open();
			return conn;
		}

		private SqlConnection ConnectionOnMaster()
		{
			var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" };
			var conn = new SqlConnection(connectionStringBuilder.ConnectionString);
			conn.Open();
			return conn;
		}

		public bool Exists()
		{
			var databaseId = ExecuteScalarOnMaster("SELECT database_id FROM sys.databases WHERE Name = '{0}'", 0, DatabaseName);
			return databaseId > 0;
		}

		public void Drop()
		{
			ExecuteNonQueryOnMaster("DROP DATABASE [{0}]", DatabaseName);
		}

		public void CreateByDbManager()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder());
			using (var conn = ConnectionOnMaster())
			{
				var creator = new DatabaseCreator(databaseFolder, conn);
				creator.CreateDatabase(DatabaseType, DatabaseName);
			}
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
			using (var conn = Connection())
			{
				var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder, conn);
				databaseVersionInformation.CreateTable();
				var schemaVersionInformation = new SchemaVersionInformation(databaseFolder);
				var schemaCreator = new DatabaseSchemaCreator(
					databaseVersionInformation,
					schemaVersionInformation,
					conn,
					databaseFolder,
					new NullLog());
				schemaCreator.Create(DatabaseType);
			}
		}

		public Backup BackupByFileCopy(string name)
		{
			var backup = new Backup();
			using (var conn = Connection())
			{
				using (var command = conn.CreateCommand())
				{
					command.CommandText = string.Format("select * from sys.sysfiles");
					using (var reader = command.ExecuteReader())
					{
						backup.Files = (from r in reader.Cast<IDataRecord>()
														let file = r.GetString(r.GetOrdinal("filename"))
														select new BackupFile { Source = file })
							.ToArray();
					}
				}
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
			using (var conn = Connection())
			{
				var versionInfo = new DatabaseVersionInformation(databaseFolder, conn);
				return versionInfo.GetDatabaseVersion();
			}
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
			ExecuteNonQueryOnMaster("ALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE", DatabaseName);
			return new GenericDisposable(() => ExecuteNonQueryOnMaster("ALTER DATABASE [{0}] SET ONLINE", DatabaseName));
		}

		private void ExecuteNonQuery(string sql, params object[] args)
		{
			using (var conn = Connection())
			{
				using (var command = conn.CreateCommand())
				{
					command.CommandText = string.Format(sql, args);
					command.ExecuteNonQuery();
				}				
			}
		}

		private void ExecuteNonQueryOnMaster(string sql, params object[] args)
		{
			using (var conn = ConnectionOnMaster())
			{
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = string.Format(sql, args);
					cmd.ExecuteNonQuery();
				}
			}
		}

		private T ExecuteScalarOnMaster<T>(string sql, T nullValue, params object[] args)
		{
			var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" };
			using (var conn = new SqlConnection(connectionStringBuilder.ConnectionString))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = string.Format(sql, args);
					var value = cmd.ExecuteScalar();
					if (value == null)
						return nullValue;
					return (T)value;
				}
			}
		}
	}
}