using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
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

		[UnitOfWork, HttpGet, Route("api/Agents/PersonDetails")]
		public virtual IHttpActionResult PersonDetails(Guid personId)
		{
			return Ok(new PersonDetailModel(_commonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(_personRepository.Get(personId))));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/For")]
		public virtual IHttpActionResult For([FromUri]AgentStateFilter filter)
		{
			return Ok(_agentViewModelBuilder.For(filter));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/StatesFor")]
		public virtual IHttpActionResult StatesFor([FromUri] AgentStateFilter filter)
		{
			return Ok(_agentStatesBuilder.For(filter));
		}
		
		[UnitOfWork, HttpGet, Route("api/Agents/InAlarmFor")]
		public virtual IHttpActionResult InAlarmFor([FromUri]AgentStateFilter filter)
		{
			return Ok(_agentStatesBuilder.InAlarmFor(filter));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/InAlarmExcludingPhoneStatesFor")]
		public virtual IHttpActionResult InAlarmExcludingPhoneStatesFor([FromUri] AgentStateFilter filter, [FromUri] IEnumerable<Guid?> excludedStateIds)
		{
			return Ok(_agentStatesBuilder.InAlarmExcludingPhoneStatesFor(filter, excludedStateIds));
		}
	}

	public class Query
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