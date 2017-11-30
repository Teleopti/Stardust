using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class AdherenceController : ApiController
	{
		private readonly AdherencePercentageViewModelBuilder _adherencePercentageViewModelBuilder;
		private readonly HistoricalAdherenceViewModelBuilder _historicalAdherenceViewModelBuilder;

		public AdherenceController(
			AdherencePercentageViewModelBuilder adherencePercentageViewModelBuilder, 
			HistoricalAdherenceViewModelBuilder historicalAdherenceViewModelBuilder)
		{
			_adherencePercentageViewModelBuilder = adherencePercentageViewModelBuilder;
			_historicalAdherenceViewModelBuilder = historicalAdherenceViewModelBuilder;
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/Adherence/ForToday")]
		public virtual IHttpActionResult ForToday(Guid personId)
		{
			var model = _adherencePercentageViewModelBuilder.Build(personId);
			return Ok(model);
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/HistoricalAdherence/For")]
		public virtual IHttpActionResult HistoricalFor(Guid personId)
		{
			var model = _historicalAdherenceViewModelBuilder.Build(personId);
			return Ok(model);
		}
	}
}
