using System;
using System.Data.SqlClient;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseHelper
	{
		public const string MasterDatabaseName = "master";
		
		private readonly ExecuteSql _usingMaster;
		private readonly ExecuteSql _usingDatabase;

		public DatabaseHelper(string connectionString, DatabaseType databaseType, IUpgradeLog log)
		{
			ConnectionString = connectionString;
			DatabaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
			DatabaseType = databaseType;
			Logger = log;

			_usingMaster = new ExecuteSql(() => openConnection(true), Logger);
			_usingDatabase = new ExecuteSql(() => openConnection(), Logger);

		}

		public DatabaseHelper(string connectionString, DatabaseType databaseType)
			: this(connectionString, databaseType, new NullLog())
		{
		}

		public IUpgradeLog Logger { set; get; }
		public string ConnectionString { get; private set; }
		public DatabaseType DatabaseType { get; private set; }
		public string DatabaseName { get; private set; }

		public string DbManagerFolderPath { get; set; }

		public string BackupNameForBackup(int dataHash)
		{
			return DatabaseType + "." + DatabaseName + "." + DatabaseVersion() + "." + OtherScriptFilesHash() + "." + dataHash + ".backup";
		}

		public string BackupNameForRestore(int dataHash)
		{
			return DatabaseType + "." + DatabaseName + "." + SchemaVersion() + "." + OtherScriptFilesHash() + "." + dataHash + ".backup";
		}

		public SqlVersion Version()
		{
			return new ServerVersionHelper(_usingMaster).Version();
		}

		public void CreateByDbManager()
		{
			dropIfExists();
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));

			var creator = new DatabaseCreator(databaseFolder, _usingMaster);
			creator.CreateDatabase(DatabaseType, DatabaseName);

			var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder, _usingDatabase);
			databaseVersionInformation.CreateTable();
		}

		public void CreateInAzureByDbManager()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));

			var creator = new DatabaseCreator(databaseFolder, _usingMaster);
			creator.CreateAzureDatabase(DatabaseType, DatabaseName);

			var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder, _usingDatabase);
			databaseVersionInformation.CreateTable();

			var azureStart = new AzureStartDDL(databaseFolder, _usingDatabase);
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
			var loginHandler = new LoginHelper(Logger, _usingMaster, databaseFolder);
			loginHandler.EnablePolicyCheck();
			return loginHandler;
		}

		public void AddPermissions(string login, string pwd, SqlVersion sqlVersion)
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));
			var permissionsHandler = new PermissionsHelper(Logger, databaseFolder, _usingDatabase);
			permissionsHandler.CreatePermissions(login, pwd, sqlVersion);
		}

		public bool HasCreateDbPermission(SqlVersion sqlVersion)
		{
			if (sqlVersion.IsAzure)
				return true;
			return Convert.ToBoolean(_usingDatabase.ExecuteScalar("SELECT IS_SRVROLEMEMBER( 'dbcreator')"));
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
					_usingDatabase.ExecuteTransactionlessNonQuery(createSql, 300);
					_usingDatabase.ExecuteTransactionlessNonQuery(dropSql, 300);
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
			return Convert.ToBoolean(_usingDatabase.ExecuteScalar("SELECT IS_SRVROLEMEMBER( 'securityadmin')"));
		}

		public void CreateSchemaByDbManager()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));

			var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder, _usingDatabase);
			var schemaVersionInformation = new SchemaVersionInformation(databaseFolder);
			var schemaCreator = new DatabaseSchemaCreator(
				databaseVersionInformation,
				schemaVersionInformation,
				_usingDatabase,
				databaseFolder,
				Logger);
			schemaCreator.Create(DatabaseType, Version());
		}

		public int DatabaseVersion()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder());
			var versionInfo = new DatabaseVersionInformation(databaseFolder, _usingDatabase);
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
			if (!Tasks().Exists(DatabaseName)) return;
			Tasks().Drop(DatabaseName);
		}

		private SqlConnection openConnection(bool masterDb = false)
		{
			var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
			if (masterDb)
				connectionStringBuilder.InitialCatalog = MasterDatabaseName;
			var conn = new SqlConnection(connectionStringBuilder.ConnectionString);
			conn.Open();
			return conn;
		}

		public AppRelatedDatabaseTasks ConfigureSystem()
		{
			return new AppRelatedDatabaseTasks(_usingDatabase);
		}

		public BackupByFileCopy BackupByFileCopy()
		{
			return new BackupByFileCopy(_usingDatabase, _usingMaster, DatabaseName);
		}

		public BackupBySql BackupBySql()
		{
			return new BackupBySql(_usingMaster, DatabaseName);
		}

		public DatabaseTasks Tasks()
		{
			return new DatabaseTasks(_usingMaster);
		}

		public void DeActivateTenantOnImport(string connectionString)
		{
			var inactivateTenant = @"UPDATE [Tenant].[Tenant] SET Active = 0, 
  ApplicationConnectionString = 'NOT IN USE, look in master Tenant db', 
  AnalyticsConnectionString  = 'NOT IN USE, look in master Tenant db', 
  AggregationConnectionString = 'NOT IN USE, look in master Tenant db'";
			_usingDatabase.ExecuteTransactionlessNonQuery(inactivateTenant, 300);
		}

		public void ReActivateTenentOnDelete(string appConnection, string analytConnection, string aggConnection)
		{
			var activateTenant = @"UPDATE [Tenant].[Tenant] SET Active = 1, 
  ApplicationConnectionString = '{0}', 
  AnalyticsConnectionString  = '{1}', 
  AggregationConnectionString = '{2}'";
			_usingDatabase.ExecuteTransactionlessNonQuery(string.Format(activateTenant,appConnection,analytConnection, aggConnection), 300);
		}
	}
}