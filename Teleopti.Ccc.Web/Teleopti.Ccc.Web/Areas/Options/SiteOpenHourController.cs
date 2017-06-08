using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;

namespace Teleopti.Ccc.Web.Areas.Options
{
	public class SiteOpenHourController : ApiController
    {
		private readonly ISiteOpenHoursPersister _siteOpenHoursPersister;

		public SiteOpenHourController(ISiteOpenHoursPersister siteOpenHoursPersister)
		{
			_siteOpenHoursPersister = siteOpenHoursPersister;
		}

		[UnitOfWork, HttpPost, Route("api/Sites/MaintainOpenHours")]
		public virtual IHttpActionResult MaintainOpenHours(IEnumerable<SiteViewModel> sites)
		{
			var persistedSitesCount = _siteOpenHoursPersister.Persist(sites);
			return Ok(persistedSitesCount);
		}
	}
}
