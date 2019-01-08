using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Wfm.Administration.Controllers
{

	[TenantTokenAuthentication]
	public class HomeController : ApiController
	{
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly SaveTenant _saveTenant;
		private readonly ITenantExists _tenantExists;
		private readonly DeleteTenant _deleteTenant;
		private readonly ICheckDatabaseVersions _checkDatabaseVersions;
		private readonly IDatabaseHelperWrapper _databaseHelperWrapper;
		private readonly RestorePersonInfoOnDetach _restorePersonInfo;

		public HomeController(ILoadAllTenants loadAllTenants, SaveTenant saveTenant, ITenantExists tenantExists,
			DeleteTenant deleteTenant, ICheckDatabaseVersions checkDatabaseVersions,
			IDatabaseHelperWrapper databaseHelperWrapper, RestorePersonInfoOnDetach restorePersonInfo)
		{
			_loadAllTenants = loadAllTenants;
			_saveTenant = saveTenant;
			_tenantExists = tenantExists;
			_deleteTenant = deleteTenant;
			_checkDatabaseVersions = checkDatabaseVersions;
			_databaseHelperWrapper = databaseHelperWrapper;
			_restorePersonInfo = restorePersonInfo;
		}


		[HttpGet]
		[TenantUnitOfWork]
		[Route("AllTenants")]
		public virtual JsonResult<IEnumerable<TenantModel>> GetAllTenants()
		{
			return Json(_loadAllTenants.Tenants().Select(t => new TenantModel
			{
				Name = t.Name,
				AnalyticsDatabase = new SqlConnectionStringBuilder( t.DataSourceConfiguration.AnalyticsConnectionString).InitialCatalog,
				AppDatabase = new SqlConnectionStringBuilder(t.DataSourceConfiguration.ApplicationConnectionString).InitialCatalog,
				AggregationDatabase = new SqlConnectionStringBuilder(t.DataSourceConfiguration.AggregationConnectionString).InitialCatalog,
				Version = _checkDatabaseVersions.GetVersions(t.DataSourceConfiguration.ApplicationConnectionString),
                Active  = t.Active
			}));
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("GetOneTenant")]
		public virtual JsonResult<TenantModel> GetOneTenant([FromBody]string name)
		{
			var tenant = _loadAllTenants.Tenants().Single(x => x.Name.Equals(name));
			var builder = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.ApplicationConnectionString);
			var builderAnal = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.AnalyticsConnectionString);
			var builderAgg = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.AggregationConnectionString);

			var maximumSessionTimeInMinutes = tenant.GetApplicationConfig(TenantApplicationConfigKey.MaximumSessionTimeInMinutes);
			return Json(new TenantModel
			{
				Name = tenant.Name,
				Id = tenant.Id,
				UseIntegratedSecurity = builder.IntegratedSecurity,
				AppDatabase = builder.InitialCatalog,
				AnalyticsDatabase = builderAnal.InitialCatalog,
				AggregationDatabase = builderAgg.InitialCatalog,
				Server = builder.DataSource,
				Version = _checkDatabaseVersions.GetVersions(tenant.DataSourceConfiguration.ApplicationConnectionString),
				CommandTimeout = int.Parse(tenant.ApplicationConfig[Environment.CommandTimeout]),
				MobileQRCodeUrl = tenant.GetApplicationConfig(TenantApplicationConfigKey.MobileQRCodeUrl),
				MaximumSessionTime =
					string.IsNullOrEmpty(maximumSessionTimeInMinutes)
						? 0
						: int.Parse(maximumSessionTimeInMinutes),
				Active = tenant.Active
			});
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("UpdateTenant")]
		public virtual JsonResult<ImportTenantResultModel> Save(UpdateTenantModel model)
		{
			return Json(_saveTenant.Execute(model));
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("api/Home/NameIsFree")]
		public virtual JsonResult<ImportTenantResultModel> NameIsFree(UpdateTenantModel model)
		{
			if (_tenantExists.CheckNewName(model.NewName, model.OriginalName))
				return Json(new ImportTenantResultModel { Message = "There is another Tenant with this name!", Success = false });

			return Json(new ImportTenantResultModel { Message = "There is no other Tenant with this name!", Success = true });
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("TenantCanBeDeleted")]
		public virtual JsonResult<TenantResultModel> TenantCanBeDeleted([FromBody]string name)
		{
			return Json(tenantCanBeDeletedInternal(name));
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("DeleteTenant")]
		public virtual JsonResult<TenantResultModel> DeleteTenant([FromBody]string name)
		{
			if (!tenantCanBeDeletedInternal(name).Success)
				return Json(new TenantResultModel {Success = false, Message = "This Tenant can not be deleted."});

			var tenant = _loadAllTenants.Tenants().FirstOrDefault(x => x.Name.Equals(name));
			_restorePersonInfo.Restore(tenant);
			_deleteTenant.Delete(tenant);
			_databaseHelperWrapper.ActivateTenantOnDelete(tenant);
			return Json(new TenantResultModel { Success = true, Message =
				$"Deleted Tenant {name}. The databases are not deleted."
			});
		}

		private TenantResultModel tenantCanBeDeletedInternal(string tenantName)
		{
			var tenant = _loadAllTenants.Tenants().FirstOrDefault(x => x.Name.Equals(tenantName));
			if (tenant == null)
				return new TenantResultModel { Success = false };

			var appDatabase =
				new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.ApplicationConnectionString).InitialCatalog;

			var localAppDb =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString).InitialCatalog;

			if (appDatabase.Equals(localAppDb))
				return new TenantResultModel { Success = false };

			return new TenantResultModel { Success = true };
		}
	}
}