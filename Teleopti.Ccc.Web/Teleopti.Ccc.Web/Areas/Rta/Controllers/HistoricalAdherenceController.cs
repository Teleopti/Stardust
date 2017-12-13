using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class HistoricalAdherenceController : ApiController
	{
		private readonly AdherencePercentageViewModelBuilder _adherencePercentageViewModelBuilder;
		private readonly HistoricalAdherenceViewModelBuilder _historicalAdherenceViewModelBuilder;
		private readonly HistoricalAdherenceDate _historicalAdherenceDate;

		public HistoricalAdherenceController(
			AdherencePercentageViewModelBuilder adherencePercentageViewModelBuilder,
			HistoricalAdherenceViewModelBuilder historicalAdherenceViewModelBuilder,
			HistoricalAdherenceDate historicalAdherenceDate)
		{
			_adherencePercentageViewModelBuilder = adherencePercentageViewModelBuilder;
			_historicalAdherenceViewModelBuilder = historicalAdherenceViewModelBuilder;
			_historicalAdherenceDate = historicalAdherenceDate;
		}

		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/HistoricalAdherence/PercentForToday")]
		public virtual IHttpActionResult ForToday(Guid personId) =>
			Ok(_adherencePercentageViewModelBuilder.Build(personId));

		[ReadModelUnitOfWork, UnitOfWork, HttpGet, Route("api/HistoricalAdherence/ForPerson")]
		public virtual IHttpActionResult HistoricalFor(Guid personId) =>
			Ok(_historicalAdherenceViewModelBuilder.Build(personId));
		
		[UnitOfWork, HttpGet, Route("api/HistoricalAdherence/MostRecentShiftDate")]
		public virtual IHttpActionResult HistoricalAdherenceDateForPerson(Guid personId) =>
			Ok(_historicalAdherenceDate.MostRecentShiftDate(personId));

	}
}