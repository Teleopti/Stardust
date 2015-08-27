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

			var result = _databaseHelperWrapper.Exists(model.ConnStringAppDatabase, DatabaseType.TeleoptiCCC7);
			if(!result.Exists)
				return Json(new ImportTenantResultModel { Success = false, Message = result.Message});
			result = _databaseHelperWrapper.Exists(model.ConnStringAnalyticsDatabase, DatabaseType.TeleoptiAnalytics);
			if (!result.Exists)
				return Json(new ImportTenantResultModel { Success = false, Message = result.Message });

			var versions =  _checkDatabaseVersions.GetVersions(new VersionCheckModel {AppConnectionString = model.ConnStringAppDatabase});
			if(!versions.AppVersionOk)
				return Json(new ImportTenantResultModel { Success = false, Message = "The databases does not have the same version." });
			
			var conflicts = _getImportUsers.GetConflictionUsers(model.ConnStringAppDatabase, model.Tenant);
		
			return Json(_import.Execute(model, conflicts));
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("api/Import/DbExists")]
		public virtual JsonResult<DbCheckResultModel> DbExists(DbCheckModel model)
		{
			var type = DatabaseType.TeleoptiCCC7;
			if (model.DbType.Equals(2))
				type = DatabaseType.TeleoptiAnalytics;

			return Json(_databaseHelperWrapper.Exists(model.DbConnectionString, type));
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
			return Json(_getImportUsers.GetConflictionUsers(model.ConnStringAppDatabase, model.Tenant));
		}

		private JsonResult<ImportTenantResultModel> isNewTenantName(string tenant)
		{
			return Json(_tenantExists.Check(tenant));
			
		}
	}
}
