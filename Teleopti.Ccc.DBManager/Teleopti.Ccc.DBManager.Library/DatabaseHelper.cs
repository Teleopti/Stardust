using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseHelper
	{
		private const string MasterDatabaseName = "master";
		
		private readonly ExecuteSql _executeMaster;
		private readonly ExecuteSql _execute;

		public DatabaseHelper(string connectionString, DatabaseType databaseType)
		{
			ConnectionString = connectionString;
			DatabaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
			DatabaseType = databaseType;
			Logger = new NullLog();

			_executeMaster = new ExecuteSql(() => openConnection(true), Logger);
			_execute = new ExecuteSql(() => openConnection(), Logger);
		}

		public IUpgradeLog Logger { set; get; }
		public string ConnectionString { get; private set; }
		public DatabaseType DatabaseType { get; private set; }
		public string DatabaseName { get; private set; }

		public ExecuteSql Executor
		{
			get { return _execute; }
		}

		public string DbManagerFolderPath { get; set; }

		public bool Exists()
		{
			return
				Convert.ToBoolean(_executeMaster.ExecuteScalar("SELECT database_id FROM sys.databases WHERE Name = @databaseName",
					parameters: new Dictionary<string, object> {{"@databaseName", DatabaseName}}));
		}

		public SqlVersion Version()
		{
			return new ServerVersionHelper(_executeMaster).Version();
		}

		public void CreateByDbManager()
		{
			dropIfExists();
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));

			var creator = new DatabaseCreator(databaseFolder, _executeMaster);
			creator.CreateDatabase(DatabaseType, DatabaseName);

			var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder, _execute);
			databaseVersionInformation.CreateTable();
		}

		public void CreateInAzureByDbManager()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));

			var creator = new DatabaseCreator(databaseFolder, _executeMaster);
			creator.CreateAzureDatabase(DatabaseType, DatabaseName);

			var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder, _execute);
			databaseVersionInformation.CreateTable();

			creator = new DatabaseCreator(databaseFolder, _execute);
			creator.ApplyAzureStartDDL(DatabaseType);
		}

		public bool LoginExists(string login, SqlVersion sqlVersion)
		{
			var sql = "SELECT 1 FROM syslogins WHERE name = @login";
			if (sqlVersion.IsAzure)
			{
				sql = "SELECT 1 FROM master.sys.sql_logins WHERE name = @login";
			}
			var result = _executeMaster.ExecuteScalar(sql, parameters: new Dictionary<string, object> {{"@login", login}});
			return Convert.ToBoolean(result);
		}

		public bool LoginCanBeCreated(string login, string password, SqlVersion sqlVersion, out string message)
		{
			try
			{
				CreateLogin(login, password, sqlVersion);
				var sql = string.Format("DROP LOGIN [{0}]", login);
				_executeMaster.ExecuteTransactionlessNonQuery(sql);
				message = "";
				return true;
			}
			catch (Exception e)
			{
				message = e.Message;
				return false;
			}
		}

		public void CreateLogin(string login, string password, SqlVersion sqlVersion)
		{
			if (LoginExists(login, sqlVersion))
				return;

			var sql = string.Format(@"CREATE LOGIN [{0}]
				WITH PASSWORD=N'{1}',
				DEFAULT_DATABASE=[master],
				DEFAULT_LANGUAGE=[us_english],
				CHECK_EXPIRATION=OFF", login, password);
			if (sqlVersion.IsAzure)
				sql = string.Format(@"CREATE LOGIN [{0}]
				WITH PASSWORD=N'{1}'", login, password);
			_executeMaster.ExecuteTransactionlessNonQuery(sql);
		}

		public void AddPermissions(string login)
		{
			if (login == "sa")
				return;
			var sql = string.Format(@"CREATE USER [{0}] FOR LOGIN [{0}]", login);
			if (DbUserExist(login))
				sql = string.Format(@"ALTER USER [{0}] WITH LOGIN = [{0}]", login);
			_execute.ExecuteNonQuery(sql);

			sql =
				string.Format(@"IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'db_executor' AND type = 'R')
	CREATE ROLE [db_executor] AUTHORIZATION [dbo]

	EXEC sp_addrolemember @rolename=N'db_executor', @membername=[{0}]
	EXEC sp_addrolemember @rolename=N'db_datawriter', @membername=[{0}]
	EXEC sp_addrolemember @rolename=N'db_datareader', @membername=[{0}]

	GRANT EXECUTE TO db_executor", login);
			_execute.ExecuteTransactionlessNonQuery(sql);
		}

		public bool DbUserExist(string sqlLogin)
		{
			const string sql = "SELECT 1 FROM sys.sysusers WHERE name = @login";
			var result = _execute.ExecuteScalar(sql, parameters: new Dictionary<string, object> {{"@login", sqlLogin}});
			return Convert.ToBoolean(result);
		}

		public bool HasCreateDbPermission(SqlVersion sqlVersion)
		{
			if (sqlVersion.IsAzure)
			{
				var dbName = Guid.NewGuid().ToString();
				try
				{
					_execute.ExecuteTransactionlessNonQuery("CREATE DATABASE [" + dbName + "]");
					_execute.ExecuteTransactionlessNonQuery("DROP DATABASE [" + dbName + "]");
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}

			return Convert.ToBoolean(_execute.ExecuteScalar("SELECT IS_SRVROLEMEMBER( 'dbcreator')"));
		}

		public bool HasCreateViewAndLoginPermission(SqlVersion sqlVersion)
		{
			if (sqlVersion.IsAzure)
			{
				const string pwd = "tT12@andSomeMore";
				var login = Guid.NewGuid().ToString().Replace("-", "#");
				var definition = sqlVersion.ProductVersion >= 12 ? "USER" : "LOGIN";
				var createSql = string.Format("CREATE {2} [{0}] WITH PASSWORD = N'{1}'", login, pwd, definition);
				var dropSql = string.Format("DROP {1} [{0}]", login, definition);
				try
				{
					_execute.ExecuteTransactionlessNonQuery(createSql);
					_execute.ExecuteTransactionlessNonQuery(dropSql);
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
			return Convert.ToBoolean(_execute.ExecuteScalar("SELECT IS_SRVROLEMEMBER( 'securityadmin')"));
		}

		public void AddSuperUser(Guid personId, string firstName, string lastName)
		{
			var sql = string.Format(@"INSERT INTO Person 
(Id, [Version], UpdatedBy, UpdatedOn, Email, Note, EmploymentNumber,FirstName, LastName, DefaultTimeZone,IsDeleted,FirstDayOfWeek)
VALUES('{2}', 1, '3F0886AB-7B25-4E95-856A-0D726EDC2A67',  GETUTCDATE(), '', '', '', '{0}', '{1}', 'UTC', 0, 1)
INSERT INTO PersonInApplicationRole
SELECT '{2}', '193AD35C-7735-44D7-AC0C-B8EDA0011E5F' , GETUTCDATE()", firstName, lastName, personId);
			_execute.ExecuteNonQuery(sql);
		}

		public void AddBusinessUnit(string name)
		{
			var sql = string.Format(@"INSERT INTO BusinessUnit
SELECT NEWID(),1, '3F0886AB-7B25-4E95-856A-0D726EDC2A67' , GETUTCDATE(), '{0}', null, 0", name);
			_execute.ExecuteNonQuery(sql);
		}

		public void CleanByAnalyticsProcedure()
		{
			_execute.ExecuteNonQuery("EXEC [mart].[etl_data_mart_delete] @DeleteAll=1");
		}

		public void CreateSchemaByDbManager()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));

			var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder, _execute);
			var schemaVersionInformation = new SchemaVersionInformation(databaseFolder);
			var schemaCreator = new DatabaseSchemaCreator(
				databaseVersionInformation,
				schemaVersionInformation,
				_execute,
				databaseFolder,
				Logger);
			schemaCreator.Create(DatabaseType);
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

		public int DatabaseVersion()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder());
			var versionInfo = new DatabaseVersionInformation(databaseFolder, _execute);
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

		private IDisposable offlineScope()
		{
			SqlConnection.ClearAllPools();
			setOffline();
			return new GenericDisposable(setOnline);
		}

		private void setOffline()
		{
			_executeMaster.ExecuteTransactionlessNonQuery(
				string.Format("ALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE", DatabaseName));
		}

		private void setOnline()
		{
			_executeMaster.ExecuteTransactionlessNonQuery(string.Format("ALTER DATABASE [{0}] SET ONLINE", DatabaseName));
		}

		private void dropIfExists()
		{
			if (!Exists()) return;

			setOnline(); // if dropping a database that is offline, the file on disk will remain!
			_executeMaster.ExecuteTransactionlessNonQuery(
				string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", DatabaseName));
			_executeMaster.ExecuteTransactionlessNonQuery(string.Format("DROP DATABASE [{0}]", DatabaseName));
		}

		private string executeShellCommandOnServer(string command)
		{
			var result = string.Empty;
			_executeMaster.ExecuteCustom(conn =>
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
						result = reader.GetString(0);
					}
				}
			});
			return result;
		}

		private SqlConnection openConnection(bool masterDb = false)
		{
			var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
			if (masterDb)
			{
				connectionStringBuilder.InitialCatalog = MasterDatabaseName;
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

			return Convert.ToBoolean(_execute.ExecuteScalar(sql));
		}
	}
}