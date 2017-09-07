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

		public OverviewController(SiteCardViewModelBuilder sites, TeamCardViewModelBuilder _teams)
		{
			_sites = sites;
			this._teams = _teams;
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Overview/SiteCards")]
		public virtual IHttpActionResult SiteCards([FromUri]Guid[] skillIds = null, [FromUri]Guid[] siteIds = null)
		{
			return Ok(_sites.Build(skillIds, siteIds));
		}
		
		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Overview/TeamCards")]
		public virtual IHttpActionResult TeamCards([FromUri]Guid siteId, [FromUri]Guid[] skillIds = null)
		{
			return Ok(skillIds.EmptyIfNull().Any() ?
					_teams.Build(siteId, skillIds) :
					_teams.Build(siteId))
				;
		}
	}
}