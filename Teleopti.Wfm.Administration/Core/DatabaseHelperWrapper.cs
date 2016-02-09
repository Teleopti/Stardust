using System;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.ApplicationConfig;
using Teleopti.Ccc.ApplicationConfig.Common;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core
{
	public class DatabaseHelperWrapper : IDatabaseHelperWrapper
	{
		private readonly IDbPathProvider _dbPathProvider;

		public DatabaseHelperWrapper(IDbPathProvider dbPathProvider)
		{
			_dbPathProvider = dbPathProvider;
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
					Message = string.Format("The connection string for {0} is not in the correct format!", dbType)
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
					Message = string.Format("Can not connect to the {0}. " + e.Message, dbType)
				};
			}

			var helper = new DatabaseHelper(databaseConnectionString, databaseType)
			{
				DbManagerFolderPath = _dbPathProvider.GetDbPath()
			};

			if (!helper.ConfigureSystem().IsCorrectDb(databaseType))
				return new DbCheckResultModel {Exists = false, Message = string.Format("The database is not a {0}.", dbType)};

			//later check so it is not used in other Tenants?
			return new DbCheckResultModel {Exists = true, Message = string.Format("{0} exists.", dbType)};

		}

		public void CreateDatabase(string connectionToNewDb, DatabaseType databaseType, string login,
			SqlVersion sqlVersion, string tenant, int tenantId)
		{

			if (sqlVersion.IsAzure && databaseType.Equals(DatabaseType.TeleoptiCCCAgg))
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
			var helper = new DatabaseHelper(connectionToNewDb, databaseType) {Logger = new TenantLogger(tenant, tenantId), DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			if (helper.Tasks().Exists(helper.DatabaseName))
				return;

			if (sqlVersion.IsAzure)
				helper.CreateInAzureByDbManager();
			else
				helper.CreateByDbManager();

			helper.CreateSchemaByDbManager();
			helper.AddPermissions(login, sqlVersion);
		}

		public void AddDatabaseUser(string connectionToNewDb, DatabaseType databaseType, string login, SqlVersion sqlVersion)
		{
			var helper = new DatabaseHelper(connectionToNewDb, databaseType) {DbManagerFolderPath = _dbPathProvider.GetDbPath()};
			helper.AddPermissions(login, sqlVersion);
		}

		public void AddSystemUser(string connectionToNewDb, Guid personId, string firstName, string lastName)
		{
			var helper = new DatabaseHelper(connectionToNewDb, DatabaseType.TeleoptiCCC7) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			helper.ConfigureSystem().AddSystemUser(personId, firstName, lastName);
		}

		public SqlVersion Version(string connectionToNewDb)
		{
			var helper = new DatabaseHelper(connectionToNewDb, DatabaseType.TeleoptiCCC7) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			return helper.Version();
		}

		public void AddBusinessUnit(string connectionToNewDb, string name)
		{
			var helper = new DatabaseHandler(new Ccc.ApplicationConfig.Common.CommandLineArgument(new string[0]));
			var defaultDataCreator = new DefaultDataCreator(name, CultureInfo.CurrentCulture, TimeZoneInfo.Local, "", "",
				helper.GetSessionFactory(connectionToNewDb),
				TenantUnitOfWorkManager.Create(connectionToNewDb));
			DefaultAggregateRoot defaultAggregateRoot = defaultDataCreator.Create();
			defaultDataCreator.Save(defaultAggregateRoot);
		}

		public bool LoginExists(string connectionToNewDb, string login, SqlVersion isAzure)
		{
			var helper = new DatabaseHelper(connectionToNewDb, DatabaseType.TeleoptiCCC7) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			return (helper.LoginTasks().LoginExists(login, isAzure));
		}

		public void CreateLogin(string connectionToNewDb, string login, string password, SqlVersion sqlVersion)
		{
			// type does not mather now
			var helper = new DatabaseHelper(connectionToNewDb, DatabaseType.TeleoptiCCC7) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			helper.LoginTasks().CreateLogin(login, password, false, sqlVersion);
		}

		public bool HasCreateDbPermission(string connectionString, SqlVersion isAzure)
		{
			var helper = new DatabaseHelper(connectionString, DatabaseType.TeleoptiCCC7) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			return helper.HasCreateDbPermission(isAzure);
		}

		public bool HasCreateViewAndLoginPermission(string connectionString, SqlVersion isAzure)
		{
			var helper = new DatabaseHelper(connectionString, DatabaseType.TeleoptiCCC7) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			return helper.HasCreateViewAndLoginPermission(isAzure);
		}

		public bool LoginCanBeCreated(string connectionString, string login, string password, SqlVersion sqlVersion,
			out string message)
		{
			var helper = new DatabaseHelper(connectionString, DatabaseType.TeleoptiCCC7) { DbManagerFolderPath = _dbPathProvider.GetDbPath() };
			return helper.LoginCanBeCreated(login, password, sqlVersion, out message);
		}
	}
}