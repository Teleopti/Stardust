using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class TeamsController : ApiController
	{
		private readonly AgentsInAlarmForTeamsViewModelBuilder _inAlarmForTeams;
		private readonly TeamViewModelBuilder _teamViewModelBuilder;
		private readonly IGetBusinessUnitId _getBusinessUnitId;

		public TeamsController(
			AgentsInAlarmForTeamsViewModelBuilder inAlarmForTeams,
			TeamViewModelBuilder teamViewModelBuilder,
			IGetBusinessUnitId getBusinessUnitId)
		{
			_inAlarmForTeams = inAlarmForTeams;
			_teamViewModelBuilder = teamViewModelBuilder;
			_getBusinessUnitId = getBusinessUnitId;
		}

		[UnitOfWork, HttpGet, Route("api/Teams/Build")]
		public virtual IHttpActionResult ForSite(Guid siteId)
		{
			return Ok(_teamViewModelBuilder.Build(siteId));
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Teams/GetOutOfAdherenceForTeamsOnSite")]
		public virtual IHttpActionResult GetOutOfAdherenceForTeamsOnSite(Guid siteId)
		{
			return Ok(_inAlarmForTeams.GetOutOfAdherenceForTeamsOnSite(siteId));
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Teams/GetOutOfAdherenceForAllTeamsOnSitesBySkill")]
		public virtual IHttpActionResult InAlarmCountForTeamsOnSiteForSkills(Guid siteId, Guid[] skillIds)
		{
			return Ok(_inAlarmForTeams.ForSkills(siteId, skillIds));
		}

		[UnitOfWork, HttpGet, Route("api/Teams/GetBusinessUnitId")]
		public virtual IHttpActionResult GetBusinessUnitId(Guid teamId)
		{
			return Ok(_getBusinessUnitId.Get(teamId));
		}
	}
}