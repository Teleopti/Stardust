using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class AgentsController : ApiController
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly ITeamRepository _teamRepository;
		private readonly IPersonRepository _personRepository;
		private readonly INow _now;
	    private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly IGetAgents _getAgents;
		private readonly IGetAgentStates _getAgentsStates;

		public AgentsController(IPermissionProvider permissionProvider,
			ITeamRepository teamRepository, 
			IPersonRepository personRepository, 
			INow now, 
			ICommonAgentNameProvider commonAgentNameProvider, 
			IGetAgents getAgents,
			IGetAgentStates getAgentsStates)
		{
			_permissionProvider = permissionProvider;
			_teamRepository = teamRepository;
			_personRepository = personRepository;
			_now = now;
		    _commonAgentNameProvider = commonAgentNameProvider;
			_getAgents = getAgents;
			_getAgentsStates = getAgentsStates;
		}

		[UnitOfWork, HttpGet, Route("api/Agents/ForTeam")]
		public virtual IHttpActionResult ForTeam(Guid teamId)
		{
			var team = _teamRepository.Get(teamId);
			var isPermitted =
				_permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview,
					_now.LocalDateOnly(), team);
			if (!isPermitted)
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			var commonAgentNameSettings = _commonAgentNameProvider.CommonAgentNameSettings;

			var today = _now.LocalDateOnly();
			var agents =
				_personRepository.FindPeopleBelongTeam(team, new DateOnlyPeriod(today, today))
					.Select(
						x =>
							new AgentViewModel
							{
								PersonId = x.Id.GetValueOrDefault(),
								Name = commonAgentNameSettings.BuildCommonNameDescription(_personRepository.Get(x.Id.GetValueOrDefault())),
								SiteId = team.Site.Id.ToString(),
								SiteName = team.Site.Description.Name,
								TeamId = team.Id.ToString(),
								TeamName = team.Description.Name
							}).ToArray();
			return Ok(agents);
		}

		[UnitOfWork, HttpGet, Route("api/Agents/Team")]
		public virtual IHttpActionResult Team(Guid personId, DateTime date)
		{
			var person = _personRepository.Get(personId);
			var team = person.MyTeam(new DateOnly(date));
			return Ok(team.Id.GetValueOrDefault());
		}

		[UnitOfWork, HttpGet, Route("api/Agents/PersonDetails")]
		public virtual IHttpActionResult PersonDetails(Guid personId)
		{
			return Ok(new PersonDetailModel(_commonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(_personRepository.Get(personId))));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/ForSites")]
		public virtual IHttpActionResult ForSites([FromUri]Guid[] siteIds)
		{
			return Ok(_getAgents.ForSites(siteIds).ToArray());
		}

		[UnitOfWork, HttpGet, Route("api/Agents/ForTeams")]
		public virtual IHttpActionResult ForTeams([FromUri]Guid[] teamIds)
		{
			return Ok(_getAgents.ForTeams(teamIds).ToArray());
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetStates")]
		public virtual IHttpActionResult GetStates(Guid teamId)
		{
			return Ok(_getAgentsStates.ForTeams(new[] { teamId }, false).ToArray());
		}


		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForTeams")]
		public virtual IHttpActionResult GetStatesForTeams([FromUri]StatesQuery query)
		{

			return Ok(new {Time = _now.UtcDateTime(), States = _getAgentsStates.ForTeams(query.Ids, false).ToArray()});
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetAlarmStatesForTeams")]
		public virtual IHttpActionResult GetAlarmStatesForTeams([FromUri]StatesQuery query)
		{
			return Ok(new { Time = _now.UtcDateTime(), States = _getAgentsStates.ForTeams(query.Ids, true).ToArray()});
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForSites")]
		public virtual IHttpActionResult GetStatesForSites([FromUri]StatesQuery query)
		{
			return Ok(new { Time = _now.UtcDateTime(), States = _getAgentsStates.ForSites(query.Ids, false).ToArray()});
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetAlarmStatesForSites")]
		public virtual IHttpActionResult GetAlarmStatesForSites([FromUri]StatesQuery query)
		{
			return Ok(new { Time = _now.UtcDateTime(), States = _getAgentsStates.ForSites(query.Ids, true).ToArray()});
		}
	}

	public class StatesQuery
	{
		public Guid[] Ids { get; set; }
    }
	
	public class PersonDetailModel
	{
		public PersonDetailModel(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
	}
}