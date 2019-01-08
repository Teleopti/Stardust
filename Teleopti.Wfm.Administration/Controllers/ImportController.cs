using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Support.Library;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
	/// <summary>
	/// Changes to this file may affect third party connections, i.e. Twillio, TalkDesk etc.
	/// Please contact CloudOps when changes are required and made. 
	/// </summary>
	[TenantTokenAuthentication]
	public class ImportController : ApiController
	{
		private readonly IDatabaseHelperWrapper _databaseHelperWrapper;
		private readonly ITenantExists _tenantExists;
		private readonly Import _import;
		private readonly ICheckDatabaseVersions _checkDatabaseVersions;
		private readonly IUpgradeLogRetriever _upgradeLogRetriever;
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly IConfigReader _configReader;

		public ImportController(IDatabaseHelperWrapper databaseHelperWrapper, ITenantExists tenantExists, Import import,
			ICheckDatabaseVersions checkDatabaseVersions, IUpgradeLogRetriever upgradeLogRetriever, ILoadAllTenants loadAllTenants, IConfigReader configReader)
		{
			_databaseHelperWrapper = databaseHelperWrapper;
			_tenantExists = tenantExists;
			_import = import;
			_checkDatabaseVersions = checkDatabaseVersions;
			_upgradeLogRetriever = upgradeLogRetriever;
			_loadAllTenants = loadAllTenants;
			_configReader = configReader;
		}

		[HttpPost]
		[TenantUnitOfWork]
		public virtual JsonResult<ImportTenantResultModel> ImportExisting(ImportDatabaseModel model)
		{
			var tenantCheck = isNewTenantName(model.Tenant);
			if (!tenantCheck.Content.Success)
				return tenantCheck;

			if(string.IsNullOrEmpty(model.Server) || string.IsNullOrEmpty(model.AnalyticsDatabase) || string.IsNullOrEmpty(model.AppDatabase) || string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
				return Json(new ImportTenantResultModel { Success = false, Message = "All properties must be filled in." });

			var appBuilder = new SqlConnectionStringBuilder {DataSource = model.Server, InitialCatalog = model.AppDatabase, UserID = model.UserName, Password = model.Password, IntegratedSecurity = model.UseIntegratedSecurity};
			
			var analBuilder = new SqlConnectionStringBuilder(appBuilder.ConnectionString) {InitialCatalog = model.AnalyticsDatabase};

			var result = _databaseHelperWrapper.Exists(appBuilder.ConnectionString, DatabaseType.TeleoptiCCC7);
			if(!result.Exists)
				return Json(new ImportTenantResultModel { Success = false, Message = result.Message});

			if(!isOtherTenantDb(appBuilder.InitialCatalog))
				return Json(new ImportTenantResultModel { Success = false, Message = "It is not allowed to import the same database again. Every Tenant must have it's own database." });
			
			var version = _checkDatabaseVersions.GetVersions(appBuilder.ConnectionString);
			if(!(version.ImportAppVersion > 360))
				return Json(new ImportTenantResultModel { Success = false, Message = "This is a version that is too early to import this way!" });

			result = _databaseHelperWrapper.Exists(analBuilder.ConnectionString, DatabaseType.TeleoptiAnalytics);
			if (!result.Exists)
				return Json(new ImportTenantResultModel { Success = false, Message = result.Message });
			var aggConnectionstring = "";
			if (!string.IsNullOrEmpty(model.AggDatabase))
			{
				var aggBuilder = new SqlConnectionStringBuilder(appBuilder.ConnectionString) { InitialCatalog = model.AggDatabase };
				result = _databaseHelperWrapper.Exists(aggBuilder.ConnectionString, DatabaseType.TeleoptiCCCAgg);
				if (!result.Exists)
					return Json(new ImportTenantResultModel { Success = false, Message = result.Message });
				aggConnectionstring = aggBuilder.ConnectionString;
			}

			var importResult = _import.Execute(model.Tenant, appBuilder.ConnectionString, analBuilder.ConnectionString, aggConnectionstring, model.AdminUser, model.AdminPassword);
			if (importResult.Success)
			{
				_databaseHelperWrapper.DeActivateTenantOnImport(appBuilder.ConnectionString);
			}
			return Json(new ImportTenantResultModel { Success = importResult.Success, Message = importResult.Message, TenantId = importResult.TenantId });
			
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("DbExists")]
		public virtual JsonResult<DbCheckResultModel> DbExists(DbCheckModel model)
		{
			if (string.IsNullOrEmpty(model.Server) || string.IsNullOrEmpty(model.Database) || string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
				return Json(new DbCheckResultModel { Exists = false, Message = "All properties must be filled in." });

			var type = DatabaseType.TeleoptiCCC7;
			if (model.DbType.Equals(2))
				type = DatabaseType.TeleoptiAnalytics;
			if (model.DbType.Equals(3))
				type = DatabaseType.TeleoptiCCCAgg;
			if(type == DatabaseType.TeleoptiCCC7 && !isOtherTenantDb(model.Database))
				return Json(new DbCheckResultModel { Exists = false, Message = "It is not allowed to import the same database again. Every Tenant must have it's own database." });

			var appBuilder = new SqlConnectionStringBuilder { DataSource = model.Server, InitialCatalog = model.Database, UserID = model.AdminUser, Password = model.AdminPassword };
			//first connect as admin to see if it is there
			var result = _databaseHelperWrapper.Exists(appBuilder.ConnectionString, type);
			if (!result.Exists)
				return Json(result);

			_databaseHelperWrapper.AddDatabaseUser(appBuilder.ConnectionString, type, model.UserName, model.Password,
				_databaseHelperWrapper.Version(appBuilder.ConnectionString));
			//check so it works now
			appBuilder = new SqlConnectionStringBuilder { DataSource = model.Server, InitialCatalog = model.Database, UserID = model.UserName, Password = model.Password };
			return Json(_databaseHelperWrapper.Exists(appBuilder.ConnectionString, type));
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("api/Import/IsNewTenant")]
		public virtual JsonResult<ImportTenantResultModel> IsNewTenant([FromBody] string tenant)
		{
			return isNewTenantName(tenant);
		}

		private JsonResult<ImportTenantResultModel> isNewTenantName(string tenant)
		{
			return Json(_tenantExists.Check(tenant));
		}
		
		[HttpPost]
		[TenantUnitOfWork]
		[Route("CheckAppLogin")]
		public virtual JsonResult<TenantResultModel> CheckAppLogin(ImportDatabaseModel model)
		{
			var connectionToNewDb = createLoginConnectionString(model);
			var version = _databaseHelperWrapper.Version(connectionToNewDb);
			if (_databaseHelperWrapper.LoginExists(connectionToNewDb, model.UserName, version))
				return Json(new TenantResultModel {Success = true, Message = "Login exists, make sure you have entered a correct password."});
			string message;
			var canCreate = _databaseHelperWrapper.LoginCanBeCreated(connectionToNewDb, model.UserName, model.Password, out message);
			if(!canCreate)
				return Json(new TenantResultModel { Success = false, Message = message });
			_databaseHelperWrapper.CreateLogin(connectionToNewDb, model.UserName, model.Password);
			return Json(new TenantResultModel {Success = true, Message = "Created new login."});
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("CheckImportAdmin")]
		public virtual JsonResult<TenantResultModel> CheckImportAdmin(ImportDatabaseModel model)
		{
			return Json(checkImportAdminInternal(createLoginConnectionString(model)));
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("GetImportLog")]
		public virtual JsonResult<IList<UpgradeLog>> GetImportLog([FromBody] int tenantId)
		{
			return Json(_upgradeLogRetriever.GetUpgradeLog(tenantId));
		}

		private string createLoginConnectionString(ImportDatabaseModel model)
		{
			if (model.Server.Contains("database.windows.net"))
			{
				var con = new SqlConnectionStringBuilder(_configReader.ConnectionString("Tenancy"));

				return new SqlConnectionStringBuilder
				{
					UserID = model.AdminUser,
					Password = model.AdminPassword,
					DataSource = model.Server,
					InitialCatalog = con.InitialCatalog,
					IntegratedSecurity = model.UseIntegratedSecurity
				}.ConnectionString;

			}

			return new SqlConnectionStringBuilder
			{
				UserID = model.AdminUser,
				Password = model.AdminPassword,
				DataSource = model.Server,
				InitialCatalog = DatabaseHelper.MasterDatabaseName,
				IntegratedSecurity = model.UseIntegratedSecurity
			}.ConnectionString;
		}

		private TenantResultModel checkImportAdminInternal(string connectionString)
		{
			var connection = new SqlConnection(connectionString);
			try
			{
				connection.Open();
			}
			catch (Exception e)
			{
				return new TenantResultModel { Success = false, Message = "Can not connect to the database. " + e.Message };
			}

			var version = _databaseHelperWrapper.Version(connectionString);
			if (!_databaseHelperWrapper.HasCreateDbPermission(connectionString))
				return new TenantResultModel { Success = false, Message = "The user does not have permission to create databases." };

			if (!_databaseHelperWrapper.HasCreateViewAndLoginPermission(connectionString))
				return new TenantResultModel { Success = false, Message = "The user does not have permission to create logins and views." };

			return new TenantResultModel { Success = true, Message = "The user does have permission to create databases, logins and views." };
		}

		private bool isOtherTenantDb(string initialCatalog)
		{
			var allOtherTenants = _loadAllTenants.Tenants();
			foreach (var otherTenant in allOtherTenants)
			{
				var builder = new SqlConnectionStringBuilder(otherTenant.DataSourceConfiguration.ApplicationConnectionString);
				if (builder.InitialCatalog == initialCatalog)
					return false;
			}
			return true;
		}
	}
}
