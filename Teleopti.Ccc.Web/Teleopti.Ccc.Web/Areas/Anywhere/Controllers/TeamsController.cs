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
	public class TeamsController : ApiController
	{
		private readonly AgentsInAlarmForTeamsViewModelBuilder _inAlarmForTeams;
		private readonly TeamViewModelBuilder _teamViewModelBuilder;

		public TeamsController(
			AgentsInAlarmForTeamsViewModelBuilder inAlarmForTeams,
			TeamViewModelBuilder teamViewModelBuilder)
		{
			_inAlarmForTeams = inAlarmForTeams;
			_teamViewModelBuilder = teamViewModelBuilder;
		}
		
		// used from anywhere
		[UnitOfWork, HttpGet, Route("api/Teams/ForSite")]
		public virtual IHttpActionResult ForSite(Guid siteId)
		{
			return Ok(_teamViewModelBuilder.Build(siteId));
		}
		
		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Teams/CardsFor")]
		public virtual IHttpActionResult CardsFor([FromUri]Guid siteId, [FromUri]Guid[] skillIds)
		{
			// check for empty because we'r scared
			return Ok(skillIds.EmptyIfNull().Any() ?
				_inAlarmForTeams.Build(siteId) : 
				_inAlarmForTeams.Build(siteId, skillIds));
		}
	}
}