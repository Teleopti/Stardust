using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Preference
{
	public class PreferenceFulfillmentChangedHandler :
		IHandleEvent<ScheduleChangedEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(PreferenceFulfillmentChangedHandler));
		private readonly IPreferenceDayRepository _preferenceDayRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IEventPublisher _eventPublisher;


		public PreferenceFulfillmentChangedHandler(IPreferenceDayRepository preferenceDayRepository, 
			IPersonRepository personRepository, 
			IEventPublisher eventPublisher)
		{
			_preferenceDayRepository = preferenceDayRepository;
			_personRepository = personRepository;
			_eventPublisher = eventPublisher;

			if (logger.IsInfoEnabled)
			{
				logger.Info("New instance of handler was created");
			}
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ScheduleChangedEvent @event)
		{
			var person = _personRepository.Get(@event.PersonId);
			if (person == null)
				return;

			var period =
				new DateTimePeriod(@event.StartDateTime, @event.EndDateTime.AddSeconds(1)).ToDateOnlyPeriod(person.PermissionInformation
					.DefaultTimeZone());
			var preferenceDays =
				_preferenceDayRepository.Find(period, person).ToLookup(p => p.RestrictionDate);
			foreach (var projectionChangedEventScheduleDay in period.DayCollection())
			{
				var preferenceDay = preferenceDays[projectionChangedEventScheduleDay].FirstOrDefault();
				if (preferenceDay == null) continue;

				_eventPublisher.Publish(new PreferenceChangedEvent
				{
					PersonId = person.Id.GetValueOrDefault(),
					RestrictionDates = new List<DateTime> {preferenceDay.RestrictionDate.Date},
					LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
					InitiatorId = @event.InitiatorId,
					LogOnDatasource = @event.LogOnDatasource,
					Timestamp = @event.Timestamp,
					ScenarioId = @event.ScenarioId
				});
			}
		}
	}
}
