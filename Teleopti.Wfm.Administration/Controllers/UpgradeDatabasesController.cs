using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class UpgradeDatabasesController : ApiController
	{
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly ICheckDatabaseVersions _checkDatabaseVersions;
		private readonly TenantUpgrader _tenantUpgrader;

		public UpgradeDatabasesController(ILoadAllTenants loadAllTenants, ICheckDatabaseVersions checkDatabaseVersions, TenantUpgrader tenantUpgrader)
		{
			_loadAllTenants = loadAllTenants;
			_checkDatabaseVersions = checkDatabaseVersions;
			_tenantUpgrader = tenantUpgrader;
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("api/UpgradeDatabases/GetVersions")]
		public virtual JsonResult<VersionResultModel> GetVersions(VersionCheckModel model)
		{
			if (string.IsNullOrEmpty(model.Server) || string.IsNullOrEmpty(model.AppDatabase) || string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
				return Json(new VersionResultModel {   AppVersionOk= false, Error = "All properties must be filled in." });

			var appBuilder = new SqlConnectionStringBuilder { DataSource = model.Server, InitialCatalog = model.AppDatabase, UserID = model.UserName, Password = model.Password };
			return Json(_checkDatabaseVersions.GetVersions(appBuilder.ConnectionString));

		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("UpgradeTenant")]
		public virtual JsonResult<TenantResultModel> UpgradeTenant(UpgradeTenantModel model)
		{
			var tenant = _loadAllTenants.Tenants().Single(x => x.Name.Equals(model.Tenant));
			_tenantUpgrader.Upgrade(tenant, model.AdminUserName, model.AdminPassword, false, model.UseIntegratedSecurity);
			return Json(new TenantResultModel {Success = true, Message = "Upgraded databases for tenant " + tenant.Name});
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("UpgradeAllTenants")]
		public virtual JsonResult<TenantResultModel> UpgradeAllTenants([FromBody]UpgradeTenantModel model)
		{
			var tenants = _loadAllTenants.Tenants().ToList();
			foreach (var tenant in tenants)
			{
				_tenantUpgrader.Upgrade(tenant, model.AdminUserName, model.AdminPassword, false, model.UseIntegratedSecurity);
			}
			
			return Json(new TenantResultModel { Success = true, Message = string.Format("Upgraded databases for {0} tenant(s).", tenants.Count()) });
		}
	}

	public class UpgradeTenantModel
	{
		public string Tenant { get; set; }
		public string AdminUserName { get; set; }
		public string AdminPassword { get; set; }
		public bool UseIntegratedSecurity { get; set; }
	}
}
