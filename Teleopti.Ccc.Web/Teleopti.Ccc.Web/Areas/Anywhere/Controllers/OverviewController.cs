using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class OverviewController : ApiController
	{
		private readonly SiteInAlarmViewModelBuilder _viewModelBuilder;
		private readonly AgentsInAlarmForTeamsViewModelBuilder _teamViewModelBuilder;

		public OverviewController(SiteInAlarmViewModelBuilder viewModelBuilder, AgentsInAlarmForTeamsViewModelBuilder teamViewModelBuilder)
		{
			_viewModelBuilder = viewModelBuilder;
			_teamViewModelBuilder = teamViewModelBuilder;
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Overview/SiteCards")]
		public virtual IHttpActionResult SiteCards([FromUri]Guid[] skillIds = null)
		{
			return Ok(skillIds.EmptyIfNull().Any() ?
					_viewModelBuilder.Build(skillIds) :
					_viewModelBuilder.Build())
				;
		}
		
		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Overview/TeamCards")]
		public virtual IHttpActionResult TeamCards([FromUri]Guid siteId, [FromUri]Guid[] skillIds = null)
		{
			return Ok(skillIds.EmptyIfNull().Any() ?
					_teamViewModelBuilder.Build(siteId, skillIds) :
					_teamViewModelBuilder.Build(siteId))
				;
		}
	}
}