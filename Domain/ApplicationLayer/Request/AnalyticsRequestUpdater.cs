using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Request
{
	public class AnalyticsRequestUpdater : 
		IHandleEvent<PersonRequestCreatedEvent>,
		IHandleEvent<PersonRequestChangedEvent>,
		IHandleEvent<PersonRequestDeletedEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsRequestUpdater));

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
		[Attempts(10)]
		public virtual void Handle(PersonRequestCreatedEvent @event)
		{
			handle(@event);
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(PersonRequestDeletedEvent @event)
		{
			handle(@event);
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(PersonRequestChangedEvent @event)
		{
			handle(@event);
		}

		private void handle(PersonRequestChangedBase @event)
		{
			var personRequest = _personRequestRepository.Get(@event.PersonRequestId);
			if (personRequest == null)
			{
				logger.Warn("Request missing from Application database, aborting.");
				return;
			}

			if (personRequest is IDeleteTag deleteTag && deleteTag.IsDeleted)
			{
				// Request has been deleted, remove from analytics
				_analyticsRequestRepository.Delete(@event.PersonRequestId);
				var requestedDays = _analyticsRequestRepository.GetAnalyticsRequestedDays(@event.PersonRequestId);
				_analyticsRequestRepository.Delete(requestedDays);
				return;
			}
			if (personRequest.Person.TerminalDate.HasValue && personRequest.RequestedDate > personRequest.Person.TerminalDate.Value.Date)
			{
				// Person has been terminated and request is after termination date, remove from analytics if it already exists
				_analyticsRequestRepository.Delete(@event.PersonRequestId);
				var requestedDays = _analyticsRequestRepository.GetAnalyticsRequestedDays(@event.PersonRequestId);
				_analyticsRequestRepository.Delete(requestedDays);
				return;
			}

			var analyticsBusinessUnit = _analyticsBusinessUnitRepository.Get(personRequest.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault());
			if (analyticsBusinessUnit == null) throw new BusinessUnitMissingInAnalyticsException();

			var personPeriod = getPersonPeriod(personRequest);

			if (personPeriod == null)
				return;

			var personTimeZone = personRequest.Person.PermissionInformation.DefaultTimeZone();
			var requestPeriod = personRequest.Request.Period;
			var numOfDays = (requestPeriod.ToDateOnlyPeriod(personTimeZone));

			var dayCollection = numOfDays.DayCollection();

			if (personRequest.Request is IShiftTradeRequest shiftTradeRequest)
				dayCollection = shiftTradeRequest.ShiftTradeSwapDetails.Select(x => x.DateFrom).ToList();

			var dayIdCollection = dayCollection.Select(x =>
			{
				var d = _analyticsDateRepository.Date(x.Date);
				if (d == null)
					throw new DateMissingInAnalyticsException(x.Date);
				return d.DateId;
			}).ToList();

			var absenceId = getAbsence(personRequest);
			var requestTypeId = getRequestType(personRequest);
			var requestStatusId = getRequestStatus(personRequest);

			if (dayIdCollection.Any())
			{
				_analyticsRequestRepository.AddOrUpdate(new AnalyticsRequest
				{
					AbsenceId = absenceId,
					RequestTypeId = requestTypeId,
					RequestDayCount = dayIdCollection.Count,
					ApplicationDatetime = TimeZoneInfo.ConvertTimeFromUtc(personRequest.CreatedOn.GetValueOrDefault(),personTimeZone),
					BusinessUnitId = analyticsBusinessUnit.BusinessUnitId,
					DatasourceUpdateDate = personRequest.UpdatedOn.GetValueOrDefault(DateTime.UtcNow),
					PersonId = personPeriod.PersonId,
					RequestCode = @event.PersonRequestId,
					RequestEndDate = numOfDays.EndDate.Date,
					RequestEndTime = requestPeriod.EndDateTimeLocal(personTimeZone),
					RequestStartDate = numOfDays.StartDate.Date,
					RequestStartDateCount = 1,
					RequestStartDateId = dayIdCollection.Min(),
					RequestStartTime = requestPeriod.StartDateTimeLocal(personTimeZone),
					RequestStatusId = requestStatusId,
					RequestedTimeMinutes = (int)(requestPeriod.EndDateTimeLocal(personTimeZone) - requestPeriod.StartDateTimeLocal(personTimeZone)).TotalMinutes
				});
			}

			var toBeRemoved =
				_analyticsRequestRepository.GetAnalyticsRequestedDays(@event.PersonRequestId)
				.Where(x => dayIdCollection.All(y => y != x.RequestDateId));
			foreach (var dateId in dayIdCollection)
			{
				_analyticsRequestRepository.AddOrUpdate(new AnalyticsRequestedDay
				{
					RequestDateId = dateId,
					AbsenceId = absenceId,
					BusinessUnitId = analyticsBusinessUnit.BusinessUnitId,
					DatasourceUpdateDate = personRequest.UpdatedOn.GetValueOrDefault(DateTime.UtcNow),
					PersonId = personPeriod.PersonId,
					RequestCode = @event.PersonRequestId,
					RequestDayCount = 1,
					RequestStatusId = requestStatusId,
					RequestTypeId = requestTypeId
				});
			}

			_analyticsRequestRepository.Delete(toBeRemoved);
		}

		private int getAbsence(IPersonRequest personRequest)
		{
			if (!(personRequest.Request is IAbsenceRequest absenceRequest)) return -1;

			var analyticsAbsence = _analyticsAbsenceRepository.Absence(absenceRequest.Absence.Id.GetValueOrDefault());
			if (analyticsAbsence != null)
				return analyticsAbsence.AbsenceId;
			throw new AbsenceMissingInAnalyticsException();
		}

		private static int getRequestType(IPersonRequest personRequest)
		{
			if (personRequest.Request is IAbsenceRequest)
				return (int)RequestType.AbsenceRequest;
			if (personRequest.Request is IShiftTradeRequest)
				return (int)RequestType.ShiftTradeRequest;
			if (personRequest.Request is IOvertimeRequest)
				return (int)RequestType.OvertimeRequest;

			return (int)RequestType.TextRequest;
		}

		private static int getRequestStatus(IPersonRequest personRequest)
		{
			if (personRequest.IsPending || personRequest.IsNew)
				return 0;
			if (personRequest.IsApproved)
				return 1;
			if (personRequest.IsDenied && !personRequest.IsWaitlisted)
				return 2;
			if(personRequest.IsCancelled)
				return 3;
			if (personRequest.IsWaitlisted)
				return 4;

			throw new ArgumentException("Unknown status of request");
		}

		private AnalyticsPersonPeriod getPersonPeriod(IPersonRequest personRequest)
		{
			var personPeriods = _analyticsPersonPeriodRepository.GetPersonPeriods(personRequest.Person.Id.GetValueOrDefault());
			var personPeriod =
				personPeriods.FirstOrDefault(
					x => x.ValidFromDate <= personRequest.RequestedDate && x.ValidToDate > personRequest.RequestedDate);
			return personPeriod;
		}
	}
}