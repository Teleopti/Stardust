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
		private readonly AgentStatesViewModelBuilder _agentStatesBuilder;

		public AgentsController(AgentStatesViewModelBuilder agentStatesBuilder)
		{
			_agentStatesBuilder = agentStatesBuilder;
		}
		
		[UnitOfWork, HttpGet, Route("api/Agents/StatesFor")]
		public virtual IHttpActionResult StatesFor([FromUri] AgentStateFilter filter)
		{
			return Ok(_agentStatesBuilder.For(filter));
		}
		
		[UnitOfWork, HttpGet, Route("api/Agents/InAlarmFor")]
		public virtual IHttpActionResult InAlarmFor([FromUri] AgentStateFilter filter)
		{
			// REMOVE ME PLOX
			filter.InAlarm = true;
			return Ok(_agentStatesBuilder.For(filter));
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
	
}