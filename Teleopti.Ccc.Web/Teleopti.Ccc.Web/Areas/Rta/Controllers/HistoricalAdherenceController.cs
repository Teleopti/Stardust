using System;
using System.Globalization;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;
using Teleopti.Wfm.Adherence.Domain.ApprovePeriodAsInAdherence;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class HistoricalAdherenceController : ApiController
	{
		private readonly HistoricalAdherenceViewModelBuilder _historicalAdherenceViewModelBuilder;
		private readonly ApprovePeriodAsInAdherenceCommandHandler _approvePeriodCommandHandler;
		private readonly HistoricalAdherenceDate _historicalAdherenceDate;
		private readonly RemoveApprovedPeriodCommandHandler _removePeriodCommandHandler;
		private readonly ICurrentAuthorization _authorization;
		private readonly IPersonRepository _persons;


		public HistoricalAdherenceController(
			HistoricalAdherenceViewModelBuilder historicalAdherenceViewModelBuilder,
			ApprovePeriodAsInAdherenceCommandHandler approvePeriodCommandHandler,
			RemoveApprovedPeriodCommandHandler removePeriodCommandHandler,
			HistoricalAdherenceDate historicalAdherenceDate,
			ICurrentAuthorization authorization,
			IPersonRepository persons)
		{
			_historicalAdherenceViewModelBuilder = historicalAdherenceViewModelBuilder;
			_approvePeriodCommandHandler = approvePeriodCommandHandler;
			_removePeriodCommandHandler = removePeriodCommandHandler;
			_historicalAdherenceDate = historicalAdherenceDate;
			_authorization = authorization;
			_persons = persons;
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
			if (!isPermitted(command.PersonId, command.StartDateTime))
				return BadRequest();

			_approvePeriodCommandHandler.Handle(command);
			return Ok();
		}

		[UnitOfWork]
		[HttpPost, Route("api/HistoricalAdherence/RemoveApprovedPeriod")]
		public virtual IHttpActionResult RemoveApprovedPeriod([FromBody] RemoveApprovedPeriodCommand command)
		{
			if (!isPermitted(command.PersonId, command.StartDateTime))
				return BadRequest();

			_removePeriodCommandHandler.Handle(command);
			return Ok();
		}

		[UnitOfWork]
		[HttpGet, Route("api/HistoricalAdherence/MostRecentShiftDate")]
		public virtual IHttpActionResult MostRecentShiftDate(Guid personId) =>
			Ok(_historicalAdherenceDate.MostRecentShiftDate(personId).Date.ToString("yyyyMMdd"));

		// remove completely?
		private bool isPermitted(Guid personId, string dateTime, string dateFormatter = "yyyy-MM-dd HH:mm:ss")
		{
			var date = new DateOnly(DateTime.ParseExact(dateTime, dateFormatter, CultureInfo.InvariantCulture, DateTimeStyles.None));
			var person = _persons.Load(personId);

			return _authorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyAdherence, date, person);
		}
	}
}