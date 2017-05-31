using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class AnywhereAgentsController : ApiController
	{
		private readonly AgentViewModelBuilder _agentViewModelBuilder;
		private readonly AgentStatesViewModelBuilder _agentStatesBuilder;

		public AnywhereAgentsController(
			AgentViewModelBuilder agentViewModelBuilder,
			AgentStatesViewModelBuilder agentStatesBuilder)
		{
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

		#region PerformanceTool
		[UnitOfWork, HttpGet, Route("api/Agents/GetStates")]
		public virtual IHttpActionResult GetStates(Guid teamId)
		{
			return Ok(_agentStatesBuilder.ForTeams(new[] { teamId }).States);
		}
		#endregion

		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForSites")]
		public virtual IHttpActionResult GetStatesForSites([FromUri] Query query)
		{
			return Ok(_agentStatesBuilder.ForSites(query.Ids));
		}

		[UnitOfWork, HttpGet, Route("api/Agents/GetStatesForTeams")]
		public virtual IHttpActionResult GetStatesForTeams([FromUri] Query query)
		{
			return Ok(_agentStatesBuilder.ForTeams(query.Ids));
		}
		
		[UnitOfWork, HttpGet, Route("api/Agents/ForTeams")]
		public virtual IHttpActionResult ForTeams([FromUri] Guid[] teamIds)
		{
			return Ok(_agentViewModelBuilder.For(new AgentStateFilter {TeamIds = teamIds}).ToArray());
		}

		[UnitOfWork, HttpGet, Route("api/Agents/ForSites")]
		public virtual IHttpActionResult ForSites([FromUri] Guid[] siteIds)
		{
			return Ok(_agentViewModelBuilder.For(new AgentStateFilter {SiteIds = siteIds}).ToArray());
		}
	}
}