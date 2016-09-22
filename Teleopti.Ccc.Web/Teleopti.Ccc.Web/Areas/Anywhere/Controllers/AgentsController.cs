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

		[UnitOfWork, HttpGet, Route("api/Agents/ForSkills")]
		public virtual IHttpActionResult ForSkills([FromUri] Guid[] skillIds)
		{
			return Ok(_agentViewModelBuilder.ForSkill(skillIds));
		}
		
		[UnitOfWork, HttpGet, Route("api/Agents/GetStates")]
		public virtual IHttpActionResult GetStates(Guid teamId)
		{
			return Ok(_agentStatesBuilder.ForTeams(new[] {teamId}).States);
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForTeams")]
		public virtual IHttpActionResult GetStatesForTeams([FromUri]Query query)
		{
			return Ok(_agentStatesBuilder.ForTeams(query.Ids));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetAlarmStatesForTeams")]
		public virtual IHttpActionResult GetAlarmStatesForTeams([FromUri]Query query)
		{
			return Ok(_agentStatesBuilder.InAlarmForTeams(query.Ids));
		}
		
		[UnitOfWork, HttpPost, Route("api/Agents/GetAlarmStatesForTeamsExcludingStateGroups")]
		public virtual IHttpActionResult GetAlarmStatesForTeamsExcludingGroups([FromBody]QueryExcludingStateGroups query)
		{
			return Ok(_agentStatesBuilder.InAlarmForTeams(query.Ids, query.ExcludedStateGroupIds));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForSites")]
		public virtual IHttpActionResult GetStatesForSites([FromUri]Query query)
		{
			return Ok(_agentStatesBuilder.ForSites(query.Ids));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetAlarmStatesForSites")]
		public virtual IHttpActionResult GetAlarmStatesForSites([FromUri]Query query)
		{
			return Ok(_agentStatesBuilder.InAlarmForSites(query.Ids));
		}

		[UnitOfWork, HttpPost, Route("api/Agents/GetAlarmStatesForSitesExcludingStateGroups")]
		public virtual IHttpActionResult GetAlarmStatesForSitesExcludingGroups([FromBody]QueryExcludingStateGroups query)
		{
			return Ok(_agentStatesBuilder.InAlarmForSites(query.Ids, query.ExcludedStateGroupIds));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForSkills")]
		public virtual IHttpActionResult GetStatesForSkills([FromUri] Query skillIds)
		{
			return Ok(_agentStatesBuilder.ForSkills(skillIds.Ids));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetAlarmStatesForSkills")]
		public virtual IHttpActionResult GetAlarmStatesForSkills([FromUri] Query skillIds)
		{
			return Ok(_agentStatesBuilder.InAlarmForSkills(skillIds.Ids));
		}

		[UnitOfWork, HttpPost, Route("api/Agents/GetAlarmStatesForSitesExcludingStateGroups")]
		public virtual IHttpActionResult GetAlarmStatesForSkillsExcludingGroups([FromBody]QueryExcludingStateGroups query)
		{
			return Ok(_agentStatesBuilder.InAlarmForSkills(query.Ids, query.ExcludedStateGroupIds));
		}
	}

	public class Query
	{
		public Guid[] Ids { get; set; }
	}

	public class QueryExcludingStateGroups
	{
		public Guid[] Ids { get; set; }
		public Guid[] ExcludedStateGroupIds { get; set; }
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