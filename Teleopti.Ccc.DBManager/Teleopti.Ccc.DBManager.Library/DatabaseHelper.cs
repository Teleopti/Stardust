using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseHelper
	{
		public DatabaseHelper(string connectionString, DatabaseType databaseType)
		{
			ConnectionString = connectionString;
			DatabaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
			DatabaseType = databaseType;
			Logger = new NullLog();
      }

		public IUpgradeLog Logger { set; get; }
		public string ConnectionString { get; private set; }
		public DatabaseType DatabaseType { get; private set; }
		public string DatabaseName { get; private set; }

		public string DbManagerFolderPath { get; set; }
		public bool Exists()
		{
			var databaseId = executeScalarOnMaster("SELECT database_id FROM sys.databases WHERE Name = '{0}'", 0, DatabaseName);
			return databaseId > 0;
		}

		public void CreateByDbManager()
		{
			dropIfExists();
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));
			using (var conn = openConnection(true))
			{
				var creator = new DatabaseCreator(databaseFolder, conn);
				creator.CreateDatabase(DatabaseType, DatabaseName);
			}
			using (var conn = openConnection())
			{
				var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder, conn);
				databaseVersionInformation.CreateTable();
			}
		}

		public void CreateInAzureByDbManager()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));
			using (var conn = openConnection(true))
			{
				var creator = new DatabaseCreator(databaseFolder, conn);
				creator.CreateAzureDatabase(DatabaseType, DatabaseName);
			}
			using (var conn = openConnection())
			{
				var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder, conn);
				databaseVersionInformation.CreateTable();

				var creator = new DatabaseCreator(databaseFolder, conn);
				creator.ApplyAzureStartDDL(DatabaseType);
			}
		}
		public bool LoginExists(string login, bool isAzure)
		{
			var sql = string.Format(@"SELECT 1 FROM syslogins WHERE name = '{0}'", login);
			if(isAzure)
				sql = string.Format(@"SELECT 1 FROM master.sys.sql_logins WHERE name = '{0}'", login);
			var result = executeScalarOnMaster(sql, 0);
			return result > 0;
		}

		public bool LoginCanBeCreated(string login, string password, bool isAzure, out string message)
		{
			try
			{
				CreateLogin(login, password, isAzure);
				var sql = string.Format("DROP LOGIN [{0}]", login);
				executeNonQueryOnMaster(sql);
				message = "";
				return true;
			}
			catch (Exception e)
			{
				message = e.Message;
				return false;
			}
		}
		public void CreateLogin(string login, string password, bool isAzure)
		{
			if(LoginExists(login, isAzure))
				return;

			var sql = string.Format(@"CREATE LOGIN [{0}]
				WITH PASSWORD=N'{1}',
				DEFAULT_DATABASE=[master],
				DEFAULT_LANGUAGE=[us_english],
				CHECK_EXPIRATION=OFF",login, password);
			if(isAzure)
				sql = string.Format(@"CREATE LOGIN [{0}]
				WITH PASSWORD=N'{1}'", login, password);
			executeNonQueryOnMaster(sql);
		}

		public void AddPermissions(string login)
		{
			if(login == "sa")
				return;
			var sql = string.Format(@"CREATE USER [{0}] FOR LOGIN [{0}]", login);
			if(DbUserExist(login))
				sql = string.Format( @"ALTER USER [{0}] WITH LOGIN = [{0}]", login);
			executeNonQuery(sql);
			
			sql = string.Format(@"IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'db_executor' AND type = 'R')
	CREATE ROLE [db_executor] AUTHORIZATION [dbo]

	EXEC sp_addrolemember @rolename=N'db_executor', @membername=[{0}]
	EXEC sp_addrolemember @rolename='db_datawriter', @membername=[{0}]
	EXEC sp_addrolemember @rolename='db_datareader', @membername=[{0}]

	GRANT EXECUTE TO db_executor", login);
			executeNonQuery(sql);
		}

		public bool DbUserExist(string sqlLogin)
		{
			var sql = string.Format(@"SELECT 1 FROM sys.sysusers WHERE name = '{0}'", sqlLogin);
			var result = executeScalar(sql, 0);
			return result > 0;
		}

		public bool HasCreateDbPermission(bool isAzure)
		{
			if (isAzure)
			{
				System.Diagnostics.Debug.Write("check create DB perm");
				var dbName = Guid.NewGuid().ToString();
				try
				{
					ExecuteSql("CREATE DATABASE [" + dbName + "]");
					ExecuteSql("DROP DATABASE [" + dbName + "]");
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}

			
			return executeScalar("SELECT IS_SRVROLEMEMBER( 'dbcreator')", 0) > 0;
		}
		
		public bool HasCreateViewAndLoginPermission(bool isAzure)
		{
			if (isAzure)
			{
				System.Diagnostics.Debug.Write("check create view perm");
				var pwd = "tT12@andSomeMore";
				var login = Guid.NewGuid().ToString().Replace("-", "#");
				var createSql = string.Format("CREATE LOGIN [{0}] WITH PASSWORD = N'{1}'", login, pwd);
				try
				{
					ExecuteSql(createSql);
					ExecuteSql("DROP LOGIN [" + login + "]");
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
			return executeScalar("SELECT IS_SRVROLEMEMBER( 'securityadmin')", 0) > 0;
		}
		public void AddSuperUser(Guid personId, string firstName, string lastName)
		{
			var sql = string.Format(@"INSERT INTO Person 
(Id, [Version], UpdatedBy, UpdatedOn, Email, Note, EmploymentNumber,FirstName, LastName, DefaultTimeZone,IsDeleted,FirstDayOfWeek)
VALUES('{2}', 1, '3F0886AB-7B25-4E95-856A-0D726EDC2A67',  GETUTCDATE(), '', '', '', '{0}', '{1}', 'UTC', 0, 1)
INSERT INTO PersonInApplicationRole
SELECT '{2}', '193AD35C-7735-44D7-AC0C-B8EDA0011E5F' , GETUTCDATE()", firstName, lastName, personId);
			executeNonQuery(sql);
		}

		public void AddBusinessUnit(string name)
		{
			var sql = string.Format(@"INSERT INTO BusinessUnit
SELECT NEWID(),1, '3F0886AB-7B25-4E95-856A-0D726EDC2A67' , GETUTCDATE(), '{0}', null, 0", name);
			executeNonQuery(sql);
		}
		public void CleanByAnalyticsProcedure()
		{
			executeNonQuery("EXEC [mart].[etl_data_mart_delete] @DeleteAll=1");
		}

		public void CreateSchemaByDbManager()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));
			using (var conn = openConnection())
			{
				var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder, conn);
				var schemaVersionInformation = new SchemaVersionInformation(databaseFolder);
				var schemaCreator = new DatabaseSchemaCreator(
					databaseVersionInformation,
					schemaVersionInformation,
					conn,
					databaseFolder,
					Logger);
				schemaCreator.Create(DatabaseType);
			}
		}

		public Backup BackupByFileCopy(string name)
		{
			var backup = new Backup();
			using (var conn = openConnection())
			{
				using (var command = conn.CreateCommand())
				{
					command.CommandText = string.Format("select * from sys.sysfiles");
					using (var reader = command.ExecuteReader())
					{
						backup.Files = (from r in reader.Cast<IDataRecord>()
							let file = r.GetString(r.GetOrdinal("filename"))
							select new BackupFile {Source = file})
							.ToArray();
					}
				}
			}
			using (offlineScope())
			{
				backup.Files.ForEach(f =>
				{
					f.Backup = f.Source + "." + name;
					var command = string.Format(@"COPY ""{0}"" ""{1}""", f.Source, f.Backup);
					var result = executeShellCommandOnServer(command);
					if (!result.Contains("1 file(s) copied."))
						throw new Exception();
				});
			}
			return backup;
		}

		public bool TryRestoreByFileCopy(Backup backup)
		{
			using (offlineScope())
			{
				return backup.Files.All(f =>
				{
					var command = string.Format(@"COPY ""{0}"" ""{1}""", f.Backup, f.Source);
					var result = executeShellCommandOnServer(command);
					return result.Contains("1 file(s) copied.");
				});
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

		public void ExecuteSql(string sql)
		{
			executeNonQuery(sql);
		}

		public int DatabaseVersion()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder());
			using (var conn = openConnection())
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

		private IDisposable offlineScope()
		{
			SqlConnection.ClearAllPools();
			setOffline();
			return new GenericDisposable(setOnline);
		}

		private void setOffline()
		{
			executeNonQueryOnMaster("ALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE", DatabaseName);
		}

		private void setOnline()
		{
			executeNonQueryOnMaster("ALTER DATABASE [{0}] SET ONLINE", DatabaseName);
		}

		private void dropIfExists()
		{
			if (!Exists()) return;

			setOnline(); // if dropping a database that is offline, the file on disk will remain!
			executeNonQueryOnMaster("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", DatabaseName);
			executeNonQueryOnMaster("DROP DATABASE [{0}]", DatabaseName);
		}

		private string executeShellCommandOnServer(string command)
		{
			using (var conn = openConnection(true))
			{
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "EXEC sp_configure 'show advanced options', 1";
					cmd.ExecuteNonQuery();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "RECONFIGURE";
					cmd.ExecuteNonQuery();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "EXEC sp_configure 'xp_cmdshell', 1";
					cmd.ExecuteNonQuery();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "RECONFIGURE";
					cmd.ExecuteNonQuery();
				}

				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "xp_cmdshell '" + command + "'";
					using (var reader = cmd.ExecuteReader())
					{
						reader.Read();
						return reader.GetString(0);
					}
				}
			}
		}

		private void executeNonQuery(string sql, params object[] args)
		{
			using (var conn = openConnection())
			{
				using (var command = conn.CreateCommand())
				{
					command.CommandText = string.Format(sql, args);
					command.ExecuteNonQuery();
				}				
			}
		}

		private void executeNonQueryOnMaster(string sql, params object[] args)
		{
			using (var conn = openConnection(true))
			{
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = string.Format(sql, args);
					cmd.ExecuteNonQuery();
				}
			}
		}

		private T executeScalarOnMaster<T>(string sql, T nullValue, params object[] args)
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

		private SqlConnection openConnection(bool masterDb = false)
		{
			var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
			if (masterDb)
			{
				connectionStringBuilder.InitialCatalog = "master";
			}
			var conn = new SqlConnection(connectionStringBuilder.ConnectionString);
			conn.Open();
			return conn;
		}

		public bool IsCorrectDb()
		{
			string sql = "SELECT count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Activity]') ";
			if (DatabaseType.Equals(DatabaseType.TeleoptiAnalytics))
				sql = "SELECT count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') ";
			if (DatabaseType.Equals(DatabaseType.TeleoptiCCCAgg))
				sql = "SELECT count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[log_object]') ";
			
         return executeScalar(sql, 0) > 0;
		}

		private T executeScalar<T>(string sql, T nullValue, params object[] args)
		{
			using (var conn = new SqlConnection(ConnectionString))
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