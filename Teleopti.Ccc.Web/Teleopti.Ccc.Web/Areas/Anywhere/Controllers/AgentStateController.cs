using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class AgentStateController : ApiController
	{
		private readonly AgentStatesViewModelBuilder _builder;

		public AgentStateController(AgentStatesViewModelBuilder builder)
		{
			_builder = builder;
		}

		[UnitOfWork, HttpGet, Route("api/AgentStates/For")]
		public virtual IHttpActionResult For([FromUri]ViewModelFilter filter)
		{
			return Ok(_builder.For(filter));
		}

		[UnitOfWork, HttpGet, Route("api/AgentStates/InAlarmFor")]
		public virtual IHttpActionResult InAlarmFor([FromUri]ViewModelFilter filter)
		{
			return Ok(_builder.InAlarmFor(filter));
		}

		[UnitOfWork, HttpGet, Route("api/AgentStates/InAlarmExcludingPhoneStatesFor")]
		public virtual IHttpActionResult InAlarmExcludingPhoneStatesFor([FromUri] ViewModelFilter filter, [FromUri] IEnumerable<Guid?> excludedStateIds)
		{
			return Ok(_builder.InAlarmExcludingPhoneStatesFor(filter, excludedStateIds));
		}
	}
}