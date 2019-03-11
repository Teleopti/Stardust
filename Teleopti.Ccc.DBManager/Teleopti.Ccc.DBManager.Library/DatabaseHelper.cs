using System;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Support.Library;
using Teleopti.Support.Library.Folders;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseHelper
	{
		public const string MasterDatabaseName = "master";

		private readonly ExecuteSql _usingMaster;
		private readonly ExecuteSql _usingDatabase;
		private readonly Lazy<SqlVersion> _sqlVersion;
		private readonly IInstallationEnvironment _installationEnvironment;

		public DatabaseHelper(
			string connectionString,
			DatabaseType databaseType,
			IInstallationEnvironment installationEnvironment)
			: this(
				connectionString,
				databaseType,
				new NullLog(),
				installationEnvironment)
		{
		}

		public DatabaseHelper(
			string connectionString,
			DatabaseType databaseType,
			IUpgradeLog log,
			IInstallationEnvironment installationEnvironment)
		{
			ConnectionString = connectionString;
			DatabaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
			DatabaseType = databaseType;
			Logger = log;
			var dataSource = new SqlConnectionStringBuilder(connectionString).DataSource;

			_usingMaster = new ExecuteSql(() =>
			{
				if (_installationEnvironment.IsAzure)
					openConnection(ForceMasterInAzure);
				return openConnection(true);
			}, Logger);

			_usingDatabase = new ExecuteSql(() => openConnection(), Logger);

			_sqlVersion = new Lazy<SqlVersion>(() => new ServerVersionHelper(_usingMaster).Version());
			_installationEnvironment = installationEnvironment;
		}


		public IUpgradeLog Logger { set; get; }
		public string ConnectionString { get; private set; }
		public DatabaseType DatabaseType { get; private set; }
		public string DatabaseName { get; private set; }

		public string DbManagerFolderPath { get; set; }
		public bool ForceMasterInAzure { get; set; } = false;

		public string BackupNameForBackup(int dataHash) =>
			DatabaseType + "." + DatabaseVersion() + "." + OtherScriptFilesHash() + "." + dataHash + ".backup";

		public string BackupNameForRestore(int dataHash) =>
			DatabaseType + "." + SchemaVersion() + "." + OtherScriptFilesHash() + "." + dataHash + ".backup";

		public SqlVersion Version() => new ServerVersionHelper(_usingMaster).Version();

		public void CreateByDbManager()
		{
			dropIfExists();
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));

			var creator = new DatabaseCreator(databaseFolder, _usingDatabase, _usingMaster);
			creator.CreateDatabase(DatabaseType, DatabaseName);

			var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder, _usingDatabase);
			databaseVersionInformation.CreateTable();
		}

		public void CreateInAzureByDbManager()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));

			var creator = new DatabaseCreator(databaseFolder, _usingDatabase, _usingMaster);
			creator.CreateAzureDatabase(DatabaseType, DatabaseName);

			var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder, _usingDatabase);
			databaseVersionInformation.CreateTable();

			var azureStart = new AzureStartDDL(databaseFolder, _usingDatabase);
			azureStart.Apply(DatabaseType);
		}

		public bool LoginCanBeCreated(string login, string password, out string message)
		{
			try
			{
				var loginHandler = LoginTasks();
				loginHandler.CreateLogin(login, password, false);
				loginHandler.DropLogin(login);
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
			var loginHandler = new LoginHelper(Logger, _usingMaster, databaseFolder, _installationEnvironment);
			loginHandler.EnablePolicyCheck();
			return loginHandler;
		}

		public void AddPermissions(string login, string pwd, SqlVersion sqlVersion)
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));
			var permissionsHandler = new PermissionsHelper(Logger, databaseFolder, _usingDatabase, _installationEnvironment);
			permissionsHandler.CreatePermissions(login, pwd);
		}

		public bool HasCreateDbPermission()
		{
			if (_installationEnvironment.IsAzure)
				return true;
			return Convert.ToBoolean(_usingDatabase.ExecuteScalar("SELECT IS_SRVROLEMEMBER( 'dbcreator')"));
		}

		public bool HasCreateViewAndLoginPermission()
		{
			if (_installationEnvironment.IsAzure)
			{
				const string pwd = "tT12@andSomeMore";
				var login = Guid.NewGuid().ToString().Replace("-", "#");
				var definition = "USER";
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
				Logger,
				_installationEnvironment);
			schemaCreator.Create(DatabaseType);
		}

		public int DatabaseVersion()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));
			var versionInfo = new DatabaseVersionInformation(databaseFolder, _usingDatabase);
			return versionInfo.GetDatabaseVersion();
		}

		public int SchemaVersion()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));
			var versionInfo = new SchemaVersionInformation(databaseFolder);
			return versionInfo.GetSchemaVersion(DatabaseType);
		}

		public int OtherScriptFilesHash()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder(DbManagerFolderPath));
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

		public ConfigureSystem ConfigureSystem()
		{
			return new ConfigureSystem(_usingDatabase);
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
			_usingDatabase.ExecuteTransactionlessNonQuery(string.Format(activateTenant, appConnection, analytConnection, aggConnection), 300);
		}

		public void RemoveOldPersonInfos()
		{
			_usingDatabase.ExecuteTransactionlessNonQuery("DELETE FROM Tenant.PersonInfo", 300);
		}
	}
}