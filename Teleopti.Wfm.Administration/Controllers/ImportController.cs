using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class ImportController : ApiController
	{
		private readonly DatabaseHelperWrapper _databaseHelperWrapper;
		private readonly ILoadAllTenants _loadAllTenants;

		public ImportController(DatabaseHelperWrapper databaseHelperWrapper, ILoadAllTenants loadAllTenants)
		{
			_databaseHelperWrapper = databaseHelperWrapper;
			_loadAllTenants = loadAllTenants;
		}

		[HttpPost]
		[TenantUnitOfWork]
		public virtual JsonResult<ImportTenantResultModel> ImportExisting(ImportDatabaseModel model)
		{
			var tenantCheck = IsNewTenant(model.Tenant);
			if (!tenantCheck.Content.Success)
				return tenantCheck;

			var result = _databaseHelperWrapper.Exists(model.ConnStringAppDatabase, DatabaseType.TeleoptiCCC7);
			if(!result.Exists)
				return Json(new ImportTenantResultModel { Success = false, Message = result.Message});
			result = _databaseHelperWrapper.Exists(model.ConnStringAnalyticsDatabase, DatabaseType.TeleoptiAnalytics);
			if (!result.Exists)
				return Json(new ImportTenantResultModel { Success = false, Message = result.Message });

			return Json(new ImportTenantResultModel { Success = true });
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
			if (string.IsNullOrEmpty(tenant))
				return Json(new ImportTenantResultModel { Message = "You must enter a new name for the Tenant!", Success = false });

			if (_loadAllTenants.TenantExists(tenant))
				return Json(new ImportTenantResultModel { Message = "There is already a Tenant with this name!", Success = false});

			return Json(new ImportTenantResultModel { Message = "There is no other Tenant with this name!", Success = true });
		}
	}
}
