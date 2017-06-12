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
		public virtual IHttpActionResult For([FromUri]AgentStateViewFilter filter)
		{
			return Ok(_builder.For(
				new AgentStateFilter
				{
					SiteIds = filter.SiteIds,
					TeamIds = filter.TeamIds,
					SkillIds = filter.SkillIds,
					InAlarm = filter.InAlarm,
					ExcludedStates = filter.ExcludedStateIds
				}));
		}

		public class AgentStateViewFilter
		{
			public IEnumerable<Guid> SiteIds { get; set; }// include
			public IEnumerable<Guid> TeamIds { get; set; }// include

			public IEnumerable<Guid> SkillIds { get; set; } // filter
			public IEnumerable<Guid?> ExcludedStateIds { get; set; } // filter
			public bool InAlarm { get; set; } // filter
		}
	}
}