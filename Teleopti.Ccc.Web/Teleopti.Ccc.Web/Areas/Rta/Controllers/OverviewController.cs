using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Wfm.Adherence.Monitor;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class OverviewController : ApiController
	{
		private readonly SiteCardViewModelBuilder _sites;

		public OverviewController(SiteCardViewModelBuilder sites)
		{
			_sites = sites;
		}

		public class SiteCardsParams
		{
			public Guid[] skillIds;
			public Guid[] siteIds;
		}
		
		[ReadModelUnitOfWork, UnitOfWork, HttpPost, Route("api/Overview/SiteCards")]
		public virtual IHttpActionResult SiteCards([FromBody] SiteCardsParams p)
		{
			return Ok(_sites.Build(p.skillIds, p.siteIds));
		}
	}
}