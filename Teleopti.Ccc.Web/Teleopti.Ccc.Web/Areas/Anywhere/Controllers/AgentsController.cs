using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class AgentsController : ApiController
	{
		private readonly IPersonRepository _personRepository;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly AgentViewModelBuilder _agentViewModelBuilder;
		private readonly AgentStatesViewModelBuilder _agentStatesBuilder;

		public AgentsController(
			IPersonRepository personRepository, 
			ICommonAgentNameProvider commonAgentNameProvider,
			AgentViewModelBuilder agentViewModelBuilder,
			AgentStatesViewModelBuilder agentStatesBuilder)
		{
			_personRepository = personRepository;
			_commonAgentNameProvider = commonAgentNameProvider;
			_agentViewModelBuilder = agentViewModelBuilder;
			_agentStatesBuilder = agentStatesBuilder;
		}

		[UnitOfWork, HttpGet, Route("api/Agents/ForTeam")]
		public virtual IHttpActionResult ForTeam(Guid teamId)
		{
			try
			{
				return Ok(_agentViewModelBuilder.ForTeam(teamId));
			}
			catch (PermissionException)
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}
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
			return Ok(_agentViewModelBuilder.ForSites(siteIds).ToArray());
		}

		[UnitOfWork, HttpGet, Route("api/Agents/ForTeams")]
		public virtual IHttpActionResult ForTeams([FromUri]Guid[] teamIds)
		{
			return Ok(_agentViewModelBuilder.ForTeams(teamIds).ToArray());
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetStates")]
		public virtual IHttpActionResult GetStates(Guid teamId)
		{
			return Ok(_agentStatesBuilder.ForTeams(new[] {teamId}, false).States);
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForTeams")]
		public virtual IHttpActionResult GetStatesForTeams([FromUri]StatesQuery query)
		{
			return Ok(_agentStatesBuilder.ForTeams(query.Ids, false));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetAlarmStatesForTeams")]
		public virtual IHttpActionResult GetAlarmStatesForTeams([FromUri]StatesQuery query)
		{
			return Ok(_agentStatesBuilder.ForTeams(query.Ids, true));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForSites")]
		public virtual IHttpActionResult GetStatesForSites([FromUri]StatesQuery query)
		{
			return Ok(_agentStatesBuilder.ForSites(query.Ids, false));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetAlarmStatesForSites")]
		public virtual IHttpActionResult GetAlarmStatesForSites([FromUri]StatesQuery query)
		{
			return Ok(_agentStatesBuilder.ForSites(query.Ids, true));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForSkill")]
		public virtual IHttpActionResult GetStatesForSkill([FromUri] SkillQuery query)
		{
			return Ok(_agentStatesBuilder.ForSkill(query.SkillId));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetAlarmStatesForSkill")]
		public virtual IHttpActionResult GetAlarmStatesForSkill([FromUri] SkillQuery query)
		{
			return Ok(_agentStatesBuilder.InAlarmForSkill(query.SkillId));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/ForSkill")]
		public virtual IHttpActionResult ForSkill([FromUri] SkillQuery query)
		{
			return Ok(_agentViewModelBuilder.ForSkill(query.SkillId));
		}
	}

	public class StatesQuery
	{
		public Guid[] Ids { get; set; }
	}

	public class SkillQuery
	{
		public Guid SkillId { get; set; }
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