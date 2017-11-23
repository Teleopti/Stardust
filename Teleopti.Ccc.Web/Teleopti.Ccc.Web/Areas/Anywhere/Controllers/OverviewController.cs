using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class OverviewController : ApiController
	{
		private readonly SiteCardViewModelBuilder _sites;
		private readonly TeamCardViewModelBuilder _teams;

		public OverviewController(SiteCardViewModelBuilder sites, TeamCardViewModelBuilder teams)
		{
			_sites = sites;
			_teams = teams;
		}

		public class Params
		{
			public Guid[] skillIds;
			public Guid[] siteIds;
		}
		
		[ReadModelUnitOfWork, UnitOfWork, HttpPost, Route("api/Overview/SiteCards")]
		public virtual IHttpActionResult SiteCards([FromBody] Params p)
		{
			return Ok(_sites.Build(p.skillIds, p.siteIds));
		}

		public class Params2
		{
			public Guid[] skillIds;
			public Guid siteId;
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpPost, Route("api/Overview/TeamCards")]
		public virtual IHttpActionResult TeamCards([FromBody] Params2 p)
		{
			return Ok(p.skillIds.EmptyIfNull().Any() ? _teams.Build(p.siteId, p.skillIds) : _teams.Build(p.siteId));
		}
	}
}