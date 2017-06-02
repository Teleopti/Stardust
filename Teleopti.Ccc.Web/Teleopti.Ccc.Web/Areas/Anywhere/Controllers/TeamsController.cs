using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
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
		
		[UnitOfWork, HttpGet, Route("api/Teams/ForSite")]
		public virtual IHttpActionResult ForSite(Guid siteId)
		{
			return Ok(_teamViewModelBuilder.Build(siteId));
		}

		[UnitOfWork, ReadModelUnitOfWork, HttpGet, Route("api/Teams/ForSkills")]
		public virtual IHttpActionResult ForSkills([FromUri]Guid siteId, [FromUri]Guid[] skillIds)
		{
			return Ok(_teamViewModelBuilder.ForSkills(siteId, skillIds));
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Teams/GetOutOfAdherenceForTeamsOnSite")]
		public virtual IHttpActionResult GetOutOfAdherenceForTeamsOnSite(Guid siteId)
		{
			return Ok(_inAlarmForTeams.GetOutOfAdherenceForTeamsOnSite(siteId));
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Teams/InAlarmCountForSkills")]
		public virtual IHttpActionResult InAlarmCountForSkills([FromUri]Guid siteId, [FromUri]Guid[] skillIds)
		{
			return Ok(_inAlarmForTeams.ForSkills(siteId, skillIds));
		}
	}
}