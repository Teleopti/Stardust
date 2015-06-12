using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class HomeController : ApiController
	{
		private readonly ILoadAllTenants _loadAllTenants;

		public HomeController(ILoadAllTenants loadAllTenants)
		{
			_loadAllTenants = loadAllTenants;
		}


		[HttpGet]
		[TenantUnitOfWork]
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
	}
}