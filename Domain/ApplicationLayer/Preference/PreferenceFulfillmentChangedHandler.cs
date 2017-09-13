using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Preference
{
	public class PreferenceFulfillmentChangedHandler :
		IHandleEvent<ProjectionChangedEvent>,
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
		public virtual void Handle(ProjectionChangedEvent @event)
		{
			var person = _personRepository.Get(@event.PersonId);
			if (person == null)
				return;

			foreach (var projectionChangedEventScheduleDay in @event.ScheduleDays)
			{
				var preferenceDay =
					_preferenceDayRepository.Find(new DateOnly(projectionChangedEventScheduleDay.Date), person).FirstOrDefault();
				if (preferenceDay == null)
					continue;
				_eventPublisher.Publish(new PreferenceChangedEvent
				{
					PreferenceDayId = preferenceDay.Id.GetValueOrDefault(),
					PersonId = person.Id.GetValueOrDefault(),
					RestrictionDate = preferenceDay.RestrictionDate.Date,
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
