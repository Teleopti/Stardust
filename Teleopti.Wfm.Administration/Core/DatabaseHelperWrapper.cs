using System;
using System.Data.SqlClient;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Support.Library;
using Teleopti.Wfm.Administration.Models;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Wfm.Administration.Core
{
	public class DatabaseHelperWrapper : IDatabaseHelperWrapper
	{
		private readonly IDbPathProvider _dbPathProvider;
		private readonly IInstallationEnvironment _installationEnvironment;
		private readonly IConfigReader _config;

		public DatabaseHelperWrapper(IDbPathProvider dbPathProvider, IInstallationEnvironment installationEnvironment, IConfigReader config)
		{
			_dbPathProvider = dbPathProvider;
			_installationEnvironment = installationEnvironment;
			_config = config;
		}

		public DbCheckResultModel Exists(string databaseConnectionString, DatabaseType databaseType)
		{
			var dbType = "Teleopti WFM application database";
			if (databaseType.Equals(DatabaseType.TeleoptiAnalytics))
				dbType = "Teleopti WFM analytics database";

			if (databaseType.Equals(DatabaseType.TeleoptiCCCAgg))
				dbType = "Teleopti WFM aggregation database";
			try
			{
				new SqlConnectionStringBuilder(databaseConnectionString);
			}
			catch (Exception)
			{
				return new DbCheckResultModel
				{
					Exists = false,
					Message = $"The connection string for {dbType} is not in the correct format!"
				};
			}

			var connection = new SqlConnection(databaseConnectionString);
			try
			{
				connection.Open();
			}
			catch (Exception e)
			{
				return new DbCheckResultModel
				{
					Exists = false,
					Message = $"Can not connect to the {dbType}. {e.Message}"
				};
			}

			var helper = new DatabaseHelper(databaseConnectionString, databaseType, _installationEnvironment)
			{
				DbManagerFolderPath = _dbPathProvider.GetDbPath()
			};

			if (!helper.ConfigureSystem().IsCorrectDb(databaseType))
				return new DbCheckResultModel { Exists = false, Message = $"The database is not a {dbType}."};

			//later check so it is not used in other Tenants?
			return new DbCheckResultModel { Exists = true, Message = $"{dbType} exists."};

		}

		public void CreateDatabase(string connectionToNewDb, DatabaseType databaseType, string login, string pwd, SqlVersion sqlVersion, string tenant, int tenantId)
		{

			if (_installationEnvironment.IsAzure && databaseType.Equals(DatabaseType.TeleoptiCCCAgg))
				return;

			try
			{
				new SqlConnectionStringBuilder(connectionToNewDb);
			}
			catch (Exception)
			{
				//return new DbCheckResultModel { Exists = false, Message = string.Format("The connection string for {0} is not in the correct format!") };
				//
			}
			var helper = new DatabaseHelper(connectionToNewDb, databaseType, _installationEnvironment) { ForceMasterInAzure = true, Logger = new TenantLogger(tenant, tenantId, _config), DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			if (helper.Tasks().Exists(helper.DatabaseName))
				return;

			if (_installationEnvironment.IsAzure)
				helper.CreateInAzureByDbManager();
			else
				helper.CreateByDbManager();

			helper.CreateSchemaByDbManager();
			helper.AddPermissions(login, pwd, sqlVersion);
		}

		public void AddDatabaseUser(string connectionToNewDb, DatabaseType databaseType, string login, string pwd, SqlVersion sqlVersion)
		{
			var helper = new DatabaseHelper(connectionToNewDb, databaseType, _installationEnvironment) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			helper.AddPermissions(login, pwd, sqlVersion);
		}

		public void AddSystemUser(string connectionToNewDb, Guid personId, string firstName, string lastName)
		{
			var helper = new DatabaseHelper(connectionToNewDb, DatabaseType.TeleoptiCCC7, _installationEnvironment) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			helper.ConfigureSystem().AddSystemUser(personId, firstName, lastName);
		}

		public void AddSystemUserToPersonInfo(string connectionToNewDb, Guid personId, string userName, string password, string tenantPassword)
		{
			var helper = new DatabaseHelper(connectionToNewDb, DatabaseType.TeleoptiCCC7, _installationEnvironment) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			helper.ConfigureSystem().AddSystemUserToPersonInfo(personId, userName, password, tenantPassword);
		}

		public SqlVersion Version(string connectionToNewDb)
		{
			var helper = new DatabaseHelper(connectionToNewDb, DatabaseType.TeleoptiCCC7, _installationEnvironment) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			return helper.Version();
		}


		public bool LoginExists(string connectionToNewDb, string login, SqlVersion version)
		{
			var helper = new DatabaseHelper(connectionToNewDb, DatabaseType.TeleoptiCCC7, _installationEnvironment) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			return helper.LoginTasks().LoginExists(login, version);
		}

		public void CreateLogin(string connectionToNewDb, string login, string password)
		{
			// type does not mather now
			var helper = new DatabaseHelper(connectionToNewDb, DatabaseType.TeleoptiCCC7, _installationEnvironment) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			helper.LoginTasks().CreateLogin(login, password, false);
		}

		public bool HasCreateDbPermission(string connectionString)
		{
			var helper = new DatabaseHelper(connectionString, DatabaseType.TeleoptiCCC7, _installationEnvironment) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			return helper.HasCreateDbPermission();
		}

		public bool HasCreateViewAndLoginPermission(string connectionString)
		{
			var helper = new DatabaseHelper(connectionString, DatabaseType.TeleoptiCCC7, _installationEnvironment) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			return helper.HasCreateViewAndLoginPermission();
		}
		
		public bool LoginCanBeCreated(string connectionString, string login, string password, out string message)
		{
			var helper = new DatabaseHelper(connectionString, DatabaseType.TeleoptiCCC7, _installationEnvironment) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			return helper.LoginCanBeCreated(login, password, out message);
		}

		public void DeActivateTenantOnImport(string connectionString)
		{
			var helper = new DatabaseHelper(connectionString, DatabaseType.TeleoptiCCC7, _installationEnvironment) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			helper.DeActivateTenantOnImport(connectionString);
		}

		public void ActivateTenantOnDelete(Tenant tenant)
		{
			var helper = new DatabaseHelper(tenant.DataSourceConfiguration.ApplicationConnectionString, DatabaseType.TeleoptiCCC7, _installationEnvironment) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			helper.ReActivateTenentOnDelete(tenant.DataSourceConfiguration.ApplicationConnectionString, tenant.DataSourceConfiguration.AnalyticsConnectionString, tenant.DataSourceConfiguration.AggregationConnectionString);
		}
	}
}