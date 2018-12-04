using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Wfm.Adherence.Historical;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.HistoricalOverview)]
	public class HistoricalOverviewController : ApiController
	{
		private readonly HistoricalOverviewViewModelBuilder _builder;

		public HistoricalOverviewController(HistoricalOverviewViewModelBuilder builder)
		{
			_builder = builder;
		}

		[UnitOfWork, HttpGet, Route("api/HistoricalOverview/Load")]
		public virtual IHttpActionResult Load([FromUri] IEnumerable<Guid> siteIds = null, [FromUri] IEnumerable<Guid> teamIds = null)
			=> Ok(_builder.Build(siteIds, teamIds));
	}
}