using System;
using System.Collections.Generic;
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

		[UnitOfWork, HttpGet, Route("api/Agents/ForTeamsAndSkills")]
		public virtual IHttpActionResult ForTeamsAndSkills([FromUri] Guid[] skillIds, [FromUri] Guid[] teamIds)
		{
			return Ok(_agentViewModelBuilder.ForSkillAndTeam(skillIds, teamIds));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/ForSitesAndSkills")]
		public virtual IHttpActionResult ForSitesAndSkills([FromUri] Guid[] skillIds, [FromUri] Guid[] siteIds)
		{
			return Ok(_agentViewModelBuilder.ForSkillAndSite(skillIds, siteIds));
		}

		

		[UnitOfWork, HttpGet, Route("api/Agents/GetStates")]
		public virtual IHttpActionResult GetStates(Guid teamId)
		{
			return Ok(_agentStatesBuilder.ForTeams(new[] {teamId}).States);
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForSites")]
		public virtual IHttpActionResult GetStatesForSites([FromUri]Query query)
		{
			return Ok(_agentStatesBuilder.ForSites(query.Ids));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForTeams")]
		public virtual IHttpActionResult GetStatesForTeams([FromUri]Query query)
		{
			return Ok(_agentStatesBuilder.ForTeams(query.Ids));
		}




		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForSkills")]
		public virtual IHttpActionResult GetStatesForSkills([FromUri] Query skillIds)
		{
			return Ok(_agentStatesBuilder.ForSkills(skillIds.Ids));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetAlarmStatesForTeams")]
		public virtual IHttpActionResult GetAlarmStatesForTeams([FromUri]Query query)
		{
			return Ok(_agentStatesBuilder.InAlarmForTeams(query.Ids));
		}
		
		[UnitOfWork, HttpGet, Route("api/Agents/GetAlarmStatesForTeamsExcludingStates")]
		public virtual IHttpActionResult GetAlarmStatesForTeamsExcludingGroups([FromUri] QueryExcludingStateGroups query)
		{
			return Ok(_agentStatesBuilder.InAlarmForTeams(query.Ids, query.ExcludedStateIds));
		}


		[UnitOfWork, HttpGet, Route("api/Agents/GetAlarmStatesForSites")]
		public virtual IHttpActionResult GetAlarmStatesForSites([FromUri]Query query)
		{
			return Ok(_agentStatesBuilder.InAlarmForSites(query.Ids));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetAlarmStatesForSitesExcludingStates")]
		public virtual IHttpActionResult GetAlarmStatesForSitesExcludingGroups([FromUri] QueryExcludingStateGroups query)
		{
			return Ok(_agentStatesBuilder.InAlarmForSites(query.Ids, query.ExcludedStateIds));
		}


		[UnitOfWork, HttpGet, Route("api/Agents/GetAlarmStatesForSkills")]
		public virtual IHttpActionResult GetAlarmStatesForSkills([FromUri] Query skillIds)
		{
			return Ok(_agentStatesBuilder.InAlarmForSkills(skillIds.Ids));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetAlarmStatesForSkillsExcludingStates")]
		public virtual IHttpActionResult GetAlarmStatesForSkillsExcludingGroups([FromUri] QueryExcludingStateGroups query)
		{
			return Ok(_agentStatesBuilder.InAlarmForSkills(query.Ids, query.ExcludedStateIds));
		}



		[UnitOfWork, HttpGet, Route("api/Agents/For")]
		public virtual IHttpActionResult For([FromUri]ViewModelFilter filter)
		{
			return Ok(_agentViewModelBuilder.For(filter));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/StatesFor")]
		public virtual IHttpActionResult StatesFor([FromUri] ViewModelFilter filter)
		{
			return Ok(_agentStatesBuilder.For(filter));
		}
		
		[UnitOfWork, HttpGet, Route("api/Agents/InAlarmFor")]
		public virtual IHttpActionResult InAlarmFor([FromUri]ViewModelFilter filter)
		{
			return Ok(_agentStatesBuilder.InAlarmFor(filter));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/InAlarmExcludingPhoneStatesFor")]
		public virtual IHttpActionResult InAlarmExcludingPhoneStatesFor([FromUri] ViewModelFilter filter, [FromUri] IEnumerable<Guid?> excludedPhoneStates)
		{
			return Ok(_agentStatesBuilder.InAlarmExcludingPhoneStatesFor(filter, excludedPhoneStates));
		}


	}
	
	public class Query
	{
		public Guid[] Ids { get; set; }
	}

	public class QueryExcludingStateGroups
	{
		public Guid[] Ids { get; set; }
		public Guid?[] ExcludedStateIds { get; set; }
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