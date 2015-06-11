using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class HomeController : ApiController
	{
		private readonly ITenantList _tenantList;

		public HomeController(ITenantList tenantList)
		{
			_tenantList = tenantList;
		}


		[HttpGet]
		[TenantUnitOfWork]
		public virtual JsonResult<IList<TenantModel>> GetAllTenants()
		{
			return Json(_tenantList.GetTenantList());
		}
	}
}