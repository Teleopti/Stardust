using System;
using System.Data.SqlClient;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseHelper
	{
		public const string MasterDatabaseName = "master";
		
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

		public string DbManagerFolderPath { get; set; }

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
				var loginHandler = LoginTasks();
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

		public LoginHelper LoginTasks()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));
			var loginHandler = new LoginHelper(Logger, _executeMaster, _execute, databaseFolder);
			loginHandler.EnablePolicyCheck();
			return loginHandler;
		}

		public void AddPermissions(string login, SqlVersion sqlVersion)
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));
			var permissionsHandler = new PermissionsHelper(Logger, databaseFolder, _execute);
			permissionsHandler.CreatePermissions(login,sqlVersion);
		}

		public bool HasCreateDbPermission(SqlVersion sqlVersion)
		{
			if (sqlVersion.IsAzure)
			{
				var dbName = Guid.NewGuid().ToString();
				try
				{
					var tasks = new DatabaseTasks(_executeMaster);
					tasks.Create(dbName);
					tasks.Drop(dbName);
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
			schemaCreator.Create(DatabaseType, Version());
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
			var tasks = new DatabaseTasks(_executeMaster);
			if (!tasks.Exists(DatabaseName)) return;

			tasks.SetOnline(DatabaseName); // if dropping a database that is offline, the file on disk will remain!
			_executeMaster.ExecuteTransactionlessNonQuery(
				string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", DatabaseName));
			tasks.Drop(DatabaseName);
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

		public AppRelatedDatabaseTasks ConfigureSystem()
		{
			return new AppRelatedDatabaseTasks(_execute);
		}

		public BackupHelper BackupHelper()
		{
			return new BackupHelper(_execute,_executeMaster,DatabaseName);
		}

		public DatabaseTasks Tasks()
		{
			return new DatabaseTasks(_executeMaster);
		}
	}
}