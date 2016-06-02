using System;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Request
{
	[EnabledBy(Toggles.ETL_SpeedUpIntradayRequest_38914)]
	public class AnalyticsRequestUpdater : 
		IHandleEvent<RequestChangedEvent>,
		IRunOnHangfire
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IAnalyticsRequestRepository _analyticsRequestRepository;
		private readonly IAnalyticsDateRepository _analyticsDateRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsAbsenceRepository _analyticsAbsenceRepository;

		public AnalyticsRequestUpdater(IPersonRequestRepository personRequestRepository, IAnalyticsRequestRepository analyticsRequestRepository, IAnalyticsDateRepository analyticsDateRepository, IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository, IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository, IAnalyticsAbsenceRepository analyticsAbsenceRepository)
		{
			_personRequestRepository = personRequestRepository;
			_analyticsRequestRepository = analyticsRequestRepository;
			_analyticsDateRepository = analyticsDateRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsAbsenceRepository = analyticsAbsenceRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		public virtual void Handle(RequestChangedEvent @event)
		{
			var personRequest = _personRequestRepository.FindPersonRequestByRequestId(@event.RequestId);
			if (personRequest == null) throw new ArgumentException("Request missing in app database");

			var businessUnit = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId);
			var personPeriod = getPersonPeriod(personRequest);
			
			var personTimeZone = personRequest.Person.PermissionInformation.DefaultTimeZone();
			var requestPeriod = personRequest.Request.Period;
			var numOfDays = (requestPeriod.ToDateOnlyPeriod(personTimeZone));

			var dayCollection = numOfDays.DayCollection();

			var shiftTradeRequest = personRequest.Request as IShiftTradeRequest;
			if (shiftTradeRequest != null)
				dayCollection = shiftTradeRequest.ShiftTradeSwapDetails.Select(x => x.DateFrom).ToList();

			var dayIdCollection = dayCollection.Select(x => _analyticsDateRepository.Date(x.Date).DateId).ToList();

			var absenceId = getAbsence(personRequest);
			var requestTypeId = getRequestType(personRequest);
			var requestStatusId = getRequestStatus(personRequest);

			_analyticsRequestRepository.AddOrUpdate(new AnalyticsRequest
			{
				AbsenceId = absenceId,
				RequestTypeId = requestTypeId,
				RequestDayCount = dayIdCollection.Count,
				ApplicationDatetime = TimeZoneInfo.ConvertTimeFromUtc(personRequest.CreatedOn.GetValueOrDefault(), personTimeZone),
				BusinessUnitId = businessUnit.BusinessUnitId,
				DatasourceUpdateDate = personRequest.UpdatedOn.GetValueOrDefault(DateTime.UtcNow),
				PersonId = personPeriod.PersonId,
				RequestCode = personRequest.Id.GetValueOrDefault(),
				RequestEndDate = numOfDays.EndDate.Date,
				RequestEndTime = requestPeriod.EndDateTimeLocal(personTimeZone),
				RequestStartDate = numOfDays.StartDate.Date,
				RequestStartDateCount = 1,
				RequestStartDateId = dayIdCollection.Min(),
				RequestStartTime = requestPeriod.StartDateTimeLocal(personTimeZone),
				RequestStatusId = requestStatusId,
				RequestedTimeMinutes = (int)(requestPeriod.EndDateTimeLocal(personTimeZone) - requestPeriod.StartDateTimeLocal(personTimeZone)).TotalMinutes
			});

			var toBeRemoved =
				_analyticsRequestRepository.GetAnalyticsRequestedDays(personRequest.Id.GetValueOrDefault())
				.Where(x => dayIdCollection.All(y => y != x.RequestDateId));
			foreach (var dateId in dayIdCollection)
			{
				_analyticsRequestRepository.AddOrUpdate(new AnalyticsRequestedDay
				{
					RequestDateId = dateId,
					AbsenceId = absenceId,
					BusinessUnitId = businessUnit.BusinessUnitId,
					DatasourceUpdateDate = personRequest.UpdatedOn.GetValueOrDefault(DateTime.UtcNow),
					PersonId = personPeriod.PersonId,
					RequestCode = personRequest.Id.GetValueOrDefault(),
					RequestDayCount = 1,
					RequestStatusId = requestStatusId,
					RequestTypeId = requestTypeId
				});
			}

			_analyticsRequestRepository.Delete(toBeRemoved);
		}

		private int getAbsence(IPersonRequest personRequest)
		{ 
			var absenceRequest = personRequest.Request as IAbsenceRequest;
			if (absenceRequest == null) return -1;

			var analyticsAbsence = _analyticsAbsenceRepository.Absences()
				.FirstOrDefault(x => x.AbsenceCode == absenceRequest.Absence.Id.GetValueOrDefault());
			if (analyticsAbsence != null)
				return analyticsAbsence.AbsenceId;
			throw new ArgumentException("Absence missing from analytics.");
		}

		private static int getRequestType(IPersonRequest personRequest)
		{
			if (personRequest.Request is IAbsenceRequest)
				return 1;
			if (personRequest.Request is IShiftTradeRequest)
				return 2;

			return 0; // TextRequest
		}

		private static int getRequestStatus(IPersonRequest personRequest)
		{
			if (personRequest.IsPending || personRequest.IsNew)
				return 0;
			if (personRequest.IsApproved)
				return 1;
			if (personRequest.IsDenied)
				return 2;

			throw new ArgumentException("Unknown status of request");
		}

		private AnalyticsPersonPeriod getPersonPeriod(IPersonRequest personRequest)
		{
			var personPeriods = _analyticsPersonPeriodRepository.GetPersonPeriods(personRequest.Person.Id.GetValueOrDefault());
			var personPeriod =
				personPeriods.FirstOrDefault(
					x => x.ValidFromDate <= personRequest.RequestedDate && x.ValidToDate > personRequest.RequestedDate);
			if (personPeriod == null)
				throw new Exception("Person period missing from analytics.");
			return personPeriod;
		}
	}
}