using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
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