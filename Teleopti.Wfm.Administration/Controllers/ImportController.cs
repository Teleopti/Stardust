using System.Data.SqlClient;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
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

		public ImportController(
			IDatabaseHelperWrapper databaseHelperWrapper, 
			ITenantExists tenantExists,
			IGetImportUsers getImportUsers, 
			ICheckDatabaseVersions checkDatabaseVersions, 
			Import import)
		{
			_databaseHelperWrapper = databaseHelperWrapper;
			_tenantExists = tenantExists;
			_getImportUsers = getImportUsers;
			_checkDatabaseVersions = checkDatabaseVersions;
			_import = import;
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

			var versions =  _checkDatabaseVersions.GetVersions(appBuilder.ConnectionString);
			if(!versions.AppVersionOk)
				return Json(new ImportTenantResultModel { Success = false, Message = "The databases does not have the same version." });
			
			var conflicts = _getImportUsers.GetConflictionUsers(appBuilder.ConnectionString, model.Tenant);
		
			return Json(_import.Execute(model.Tenant, appBuilder.ConnectionString, analBuilder.ConnectionString, conflicts));
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
	}
}
