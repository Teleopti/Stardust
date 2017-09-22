using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Requests.Controller
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebRequests)]
	public class SiteWithOpenHourController : ApiController
	{
		private readonly ISiteWithOpenHourProvider _siteWithOpenHourProvider;

		public SiteWithOpenHourController(ISiteWithOpenHourProvider siteWithOpenHourProvider)
		{
			_siteWithOpenHourProvider = siteWithOpenHourProvider;
		}

		[HttpGet, Route("api/Requests/sitesWithOpenHour"), UnitOfWork]
		public virtual IEnumerable<SiteViewModel> GetSitesWithOpenHour()
		{
			return _siteWithOpenHourProvider.GetSitesWithOpenHour();
		}
	}
}