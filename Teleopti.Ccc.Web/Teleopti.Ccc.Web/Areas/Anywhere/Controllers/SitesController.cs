using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class SitesController : ApiController
	{
		private readonly SiteInAlarmViewModelBuilder _viewModelBuilder;

		public SitesController(SiteInAlarmViewModelBuilder viewModelBuilder)
		{
			_viewModelBuilder = viewModelBuilder;
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Sites/GetOutOfAdherenceForAllSites")]
		public virtual IHttpActionResult GetOutOfAdherenceForAllSites()
		{
			return Ok(_viewModelBuilder.Build());
		}

		[UnitOfWork, ReadModelUnitOfWork, HttpGet, Route("api/Sites/InAlarmCountForSkills")]
		public virtual IHttpActionResult InAlarmCountForSkills([FromUri]Guid[] skillIds)
		{
			return Ok(_viewModelBuilder.Build(skillIds));
		}
	}
}