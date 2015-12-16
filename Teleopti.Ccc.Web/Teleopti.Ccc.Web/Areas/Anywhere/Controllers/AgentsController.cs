using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
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
		private readonly INow _date;
	    private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;
		private readonly IGetAgents _getAgents;
		private readonly IGetAgentStates _getAgentsStates;

		public AgentsController(IPermissionProvider permissionProvider,
			ITeamRepository teamRepository, 
			IPersonRepository personRepository, 
			INow date, 
			ICommonAgentNameProvider commonAgentNameProvider, 
			IAgentStateReadModelReader agentStateReadModelReader,
			IGetAgents getAgents,
			IGetAgentStates getAgentsStates)
		{
			_permissionProvider = permissionProvider;
			_teamRepository = teamRepository;
			_personRepository = personRepository;
			_date = date;
		    _commonAgentNameProvider = commonAgentNameProvider;
		    _agentStateReadModelReader = agentStateReadModelReader;
			_getAgents = getAgents;
			_getAgentsStates = getAgentsStates;
		}

		[HttpGet, Route("api/Agents/GetStates")]
		public IHttpActionResult GetStates(Guid teamId)
		{
			var states = _agentStateReadModelReader.LoadForTeam(teamId).Select(x => new AgentStateViewModel
			{
				PersonId = x.PersonId,
				State = x.State,
				StateStartTime = getNullableUtcDatetime(x.StateStartTime),
				Activity = x.Scheduled,
				NextActivity = x.ScheduledNext,
				NextActivityStartTime = getNullableUtcDatetime(x.NextStart),
				Alarm = x.AlarmName,
				AlarmStart = getNullableUtcDatetime(x.RuleStartTime),
				AlarmColor = ColorTranslator.ToHtml(Color.FromArgb(x.Color ?? 0)),
				TimeInState = calculateTimeInState(x.StateStartTime)
			}).ToArray();

			return Ok(states);
		}

		private static DateTime? getNullableUtcDatetime(DateTime? dateTime)
		{
			return dateTime == null ? (DateTime?)null : DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);
		}

		private int calculateTimeInState(DateTime? dateTime)
		{
			var stateStartTime = getNullableUtcDatetime(dateTime);
			return stateStartTime.HasValue ? (int) (_date.UtcDateTime() - stateStartTime.Value).TotalSeconds : 0;
		}

		[UnitOfWork, HttpGet, Route("api/Agents/ForTeam")]
		public virtual IHttpActionResult ForTeam(Guid teamId)
		{
			var team = _teamRepository.Get(teamId);
			var isPermitted =
				_permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview,
					_date.LocalDateOnly(), team);
			if (!isPermitted)
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}
			var today = _date.LocalDateOnly();
			var agents =
				_personRepository.FindPeopleBelongTeam(team, new DateOnlyPeriod(today, today))
					.Select(
						x =>
							new AgentViewModel
							{
								PersonId = x.Id.GetValueOrDefault(),
                                Name = _commonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(_personRepository.Get(x.Id.GetValueOrDefault())),
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

		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForSites")]
		public virtual IHttpActionResult GetStatesForSites([FromUri]Guid[] siteIds, bool? isInAlarm)
		{
			return Ok(_getAgentsStates.ForSites(siteIds, isInAlarm).ToArray());
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForTeams")]
		public virtual IHttpActionResult GetStatesForTeams([FromUri]Guid[] teamIds, bool? isInAlarm)
		{
			return Ok(_getAgentsStates.ForTeams(teamIds, isInAlarm).ToArray());
		}
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