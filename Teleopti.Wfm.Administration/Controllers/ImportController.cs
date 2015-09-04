using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Support.Security;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class ImportController : ApiController
	{
		private readonly IDatabaseHelperWrapper _databaseHelperWrapper;
		private readonly ITenantExists _tenantExists;
		private readonly IGetImportUsers _getImportUsers;
		private readonly ICheckDatabaseVersions _checkDatabaseVersions;
		private readonly Import _import;
		private readonly TenantUpgrader _tenantUpgrader;
		private readonly bool isAzure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));

		public ImportController(
			IDatabaseHelperWrapper databaseHelperWrapper, 
			ITenantExists tenantExists,
			IGetImportUsers getImportUsers, 
			ICheckDatabaseVersions checkDatabaseVersions, 
			Import import ,
			TenantUpgrader tenantUpgrader
         )
		{
			_databaseHelperWrapper = databaseHelperWrapper;
			_tenantExists = tenantExists;
			_getImportUsers = getImportUsers;
			_checkDatabaseVersions = checkDatabaseVersions;
			_import = import;
			_tenantUpgrader = tenantUpgrader;
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

			var appBuilder = new SqlConnectionStringBuilder {DataSource = model.Server, InitialCatalog = model.AppDatabase, UserID = model.UserName, Password = model.Password};
			var analBuilder = new SqlConnectionStringBuilder(appBuilder.ConnectionString) {InitialCatalog = model.AnalyticsDatabase};

			var result = _databaseHelperWrapper.Exists(appBuilder.ConnectionString, DatabaseType.TeleoptiCCC7);
			if(!result.Exists)
				return Json(new ImportTenantResultModel { Success = false, Message = result.Message});
			result = _databaseHelperWrapper.Exists(analBuilder.ConnectionString, DatabaseType.TeleoptiAnalytics);
			if (!result.Exists)
				return Json(new ImportTenantResultModel { Success = false, Message = result.Message });

			var conflicts = _getImportUsers.GetConflictionUsers(appBuilder.ConnectionString, model.Tenant);

			var importResult = _import.Execute(model.Tenant, appBuilder.ConnectionString, analBuilder.ConnectionString, conflicts);
			if (!importResult.Success)
				return Json(new ImportTenantResultModel { Success = false, Message = importResult.Message });

			var versions = _checkDatabaseVersions.GetVersions(appBuilder.ConnectionString);
			if (!versions.AppVersionOk)
			{
				_tenantUpgrader.Upgrade(importResult.Tenant, model.AdminUser, model.AdminPassword, true);
			}

			return Json(new ImportTenantResultModel { Success = false, Message = importResult.Message });
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
			var appBuilder = new SqlConnectionStringBuilder { DataSource = model.Server, InitialCatalog = model.Database, UserID = model.UserName, Password = model.Password };

			return Json(_databaseHelperWrapper.Exists(appBuilder.ConnectionString, type));
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("api/Import/IsNewTenant")]
		public virtual JsonResult<ImportTenantResultModel> IsNewTenant([FromBody] string tenant)
		{
			return isNewTenantName(tenant);
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("api/Import/Conflicts")]
		public virtual JsonResult<ConflictModel> Conflicts(ImportDatabaseModel model)
		{
			var appBuilder = new SqlConnectionStringBuilder { DataSource = model.Server, InitialCatalog = model.AppDatabase, UserID = model.UserName, Password = model.Password };
			return Json(_getImportUsers.GetConflictionUsers(appBuilder.ConnectionString, model.Tenant));
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
			if (_databaseHelperWrapper.LoginExists(createLoginConnectionString(model), model.UserName, isAzure))
				return Json(new TenantResultModel {Success = true, Message = "Login exists."});
			var message = "";
			var canCreate = _databaseHelperWrapper.LoginCanBeCreated(createLoginConnectionString(model), model.UserName, model.Password, isAzure, out message);
			if(!canCreate)
				return Json(new TenantResultModel { Success = true, Message = message });
			_databaseHelperWrapper.CreateLogin(createLoginConnectionString(model), model.UserName, model.Password,isAzure);
			return Json(new TenantResultModel {Success = true, Message = "Created new login."});
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("CheckImportAdmin")]
		public virtual JsonResult<TenantResultModel> CheckImportAdmin(ImportDatabaseModel model)
		{
			return Json(checkImportAdminInternal(createLoginConnectionString(model)));
		}

		private string createLoginConnectionString(ImportDatabaseModel model)
		{
			return new SqlConnectionStringBuilder
			{
				UserID = model.AdminUser,
				Password = model.AdminPassword,
				DataSource = model.Server,
				InitialCatalog = "master",
				IntegratedSecurity = false
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

			if (!_databaseHelperWrapper.HasCreateDbPermission(connectionString, isAzure))
				return new TenantResultModel { Success = false, Message = "The user does not have permission to create databases." };

			if (!_databaseHelperWrapper.HasCreateViewAndLoginPermission(connectionString, isAzure))
				return new TenantResultModel { Success = false, Message = "The user does not have permission to create logins and views." };

			return new TenantResultModel { Success = true, Message = "The user does have permission to create databases, logins and views." };
		}
	}
}
