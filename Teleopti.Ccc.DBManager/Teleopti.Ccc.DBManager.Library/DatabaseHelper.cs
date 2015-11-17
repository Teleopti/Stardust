using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

			var azureStart = new AzureStartDDL(databaseFolder, _execute);
			azureStart.Apply(DatabaseType);
		}

		public bool LoginCanBeCreated(string login, string password, SqlVersion sqlVersion, out string message)
		{
			try
			{
				var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));
				var loginHandler = new LoginHelper(Logger, _executeMaster, _execute, databaseFolder);
				loginHandler.EnablePolicyCheck();
				loginHandler.CreateLogin(login, password, false, sqlVersion);
				loginHandler.DropLogin(login, sqlVersion);
				message = "";
				return true;
			}
			catch (Exception e)
			{
				message = e.Message;
				return false;
			}
		}

		public bool LoginExists(string login, SqlVersion sqlVersion)
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));
			var loginHandler = new LoginHelper(Logger, _executeMaster, _execute, databaseFolder);
			return loginHandler.LoginExists(login, sqlVersion);
		}

		public void CreateLogin(string login, string password, SqlVersion sqlVersion)
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));
			var loginHandler = new LoginHelper(Logger, _executeMaster, _execute, databaseFolder);
			loginHandler.EnablePolicyCheck();
			loginHandler.CreateLogin(login,password,false,sqlVersion);
		}

		public void AddPermissions(string login, SqlVersion sqlVersion)
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));
			var permissionsHandler = new PermissionsHelper(Logger, databaseFolder, _execute);
			permissionsHandler.CreatePermissions(login,sqlVersion);
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

		private void dropIfExists()
		{
			if (!Exists()) return;

			new OnlineHelper(_executeMaster).SetOnline(DatabaseName); // if dropping a database that is offline, the file on disk will remain!
			_executeMaster.ExecuteTransactionlessNonQuery(
				string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", DatabaseName));
			_executeMaster.ExecuteTransactionlessNonQuery(string.Format("DROP DATABASE [{0}]", DatabaseName));
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

		public BackupHelper BackupHelper()
		{
			return new BackupHelper(_execute,_executeMaster,DatabaseName);
		}
	}
}