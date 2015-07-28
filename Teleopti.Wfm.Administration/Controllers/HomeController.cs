using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{

	[TenantTokenAuthentication]
	public class HomeController : ApiController
	{
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly SaveTenant _saveTenant;
		private readonly ITenantExists _tenantExists;

		public HomeController(ILoadAllTenants loadAllTenants, SaveTenant saveTenant, ITenantExists tenantExists)
		{
			_loadAllTenants = loadAllTenants;
			_saveTenant = saveTenant;
			_tenantExists = tenantExists;
		}


		[HttpGet]
		[TenantUnitOfWork]
		[Route("api/Home/GetAllTenants")]
		public virtual JsonResult<IEnumerable<TenantModel>> GetAllTenants()
		{
			return Json(_loadAllTenants.Tenants().Select(t => new TenantModel
			{
				Name = t.Name,
				Id = -1000, //behövs denna?
				AnalyticsDatabase = t.AnalyticsConnectionString,
				AppDatabase = t.ApplicationConnectionString
			}));
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("api/Home/GetOneTenant")]
		public virtual JsonResult<TenantModel> GetOneTenant([FromBody]string name)
		{
			return Json(_loadAllTenants.Tenants().Where(x => x.Name.Equals(name)).Select(t => new TenantModel
			{
				Name = t.Name,
				Id = -1000, //behövs denna?
				AnalyticsDatabase = t.AnalyticsConnectionString,
				AppDatabase = t.ApplicationConnectionString
			}).FirstOrDefault());
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("api/Home/Save")]
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
	}
}