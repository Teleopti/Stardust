using System;
using System.Globalization;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class HistoricalAdherenceController : ApiController
	{
		private readonly HistoricalAdherenceViewModelBuilder _historicalAdherenceViewModelBuilder;
		private readonly ApprovePeriodAsInAdherenceCommandHandler _approvePeriodCommandHandler;
		private readonly HistoricalAdherenceDate _historicalAdherenceDate;

		public HistoricalAdherenceController(
			HistoricalAdherenceViewModelBuilder historicalAdherenceViewModelBuilder,
			ApprovePeriodAsInAdherenceCommandHandler approvePeriodCommandHandler,
			HistoricalAdherenceDate historicalAdherenceDate)
		{
			_historicalAdherenceViewModelBuilder = historicalAdherenceViewModelBuilder;
			_approvePeriodCommandHandler = approvePeriodCommandHandler;
			_historicalAdherenceDate = historicalAdherenceDate;
		}

		[ReadModelUnitOfWork, UnitOfWork]
		[HttpGet, Route("api/HistoricalAdherence/ForPerson")]
		public virtual IHttpActionResult ForPerson(Guid personId, string date)
		{
			var dateTime = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
			return Ok(_historicalAdherenceViewModelBuilder.Build(personId, new DateOnly(dateTime)));
		}
		
		[UnitOfWork]
		[HttpPost, Route("api/HistoricalAdherence/ApprovePeriod")]
		public virtual IHttpActionResult ApprovePeriod([FromBody] ApprovePeriodAsInAdherenceCommand command)
		{
			_approvePeriodCommandHandler.Handle(command);
			return Ok();
		}
		
		[UnitOfWork]
		[HttpGet, Route("api/HistoricalAdherence/MostRecentShiftDate")]
		public virtual IHttpActionResult MostRecentShiftDate(Guid personId) =>
			Ok(_historicalAdherenceDate.MostRecentShiftDate(personId).Date.ToString("yyyyMMdd"));
	}

}