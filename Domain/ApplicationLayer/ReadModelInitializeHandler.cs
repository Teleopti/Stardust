using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class ReadModelInitializeHandler : 
		IHandleEvent<InitialLoadScheduleProjectionEvent>,
		IRunOnHangfire
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private readonly IPersonScheduleDayReadModelPersister _personScheduleDayReadModelRepository;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IDistributedLockAcquirer _distributedLockAcquirer;
		private readonly IEventPublisher _eventPublisher;

		private IList<IPerson> _people;
		private IScenario _defaultScenario;
		private DateOnlyPeriod _period;
		private DateTimePeriod _utcPeriod;

		public ReadModelInitializeHandler(IPersonRepository personRepository, IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister, IScheduleDayReadModelRepository scheduleDayReadModelRepository, IPersonScheduleDayReadModelPersister personScheduleDayReadModelRepository, ICurrentScenario scenarioRepository, IEventPublisher eventPublisher, IDistributedLockAcquirer distributedLockAcquirer)
		{
			_personRepository = personRepository;
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_scenarioRepository = scenarioRepository;
			_eventPublisher = eventPublisher;
			_distributedLockAcquirer = distributedLockAcquirer;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(InitialLoadScheduleProjectionEvent @event)
		{
			_distributedLockAcquirer.TryLockForTypeOfAnd(this, @event.LogOnBusinessUnitId.ToString(), () =>
			// Use a lock for each business unit to only run this once for each BU
			{
				var messages = new List<ScheduleChangedEventBase>();
				var projectionModelInitialized = _scheduleProjectionReadOnlyPersister.IsInitialized();
				var scheduleDayModelInitialized = _scheduleDayReadModelRepository.IsInitialized();
				var personScheduleDayModelInitialized = _personScheduleDayReadModelRepository.IsInitialized();

				if (projectionModelInitialized && scheduleDayModelInitialized && personScheduleDayModelInitialized) return;

				loadPeopleAndScenario(@event.StartDays, @event.EndDays);

				if (!projectionModelInitialized && !scheduleDayModelInitialized && !personScheduleDayModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleChangedEvent>(@event));

					projectionModelInitialized = true;
					scheduleDayModelInitialized = true;
					personScheduleDayModelInitialized = true;
				}
				if (!projectionModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleInitializeTriggeredEventForScheduleProjection>(@event));
				}
				if (!scheduleDayModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleInitializeTriggeredEventForScheduleDay>(@event));
				}
				if (!personScheduleDayModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleInitializeTriggeredEventForPersonScheduleDay>(@event));
				}
				messages.ForEach(m => _eventPublisher.Publish(m));

				// #43841 keep the lock for 5 extra minutes, to avoid duplicate event.
				Thread.Sleep(TimeSpan.FromMinutes(5));
			});
		}

		private void loadPeopleAndScenario(int startDays, int endDays)
		{
			_people = _personRepository.LoadAll();
			_defaultScenario = _scenarioRepository.Current();
			_period = new DateOnlyPeriod(DateOnly.Today.AddDays(startDays), DateOnly.Today.AddDays(endDays));
			_utcPeriod = _period.ToDateTimePeriod(TimeZoneInfo.Utc);
		}

		private IEnumerable<T> initialLoad<T>(InitialLoadScheduleProjectionEvent message) where T : ScheduleChangedEventBase, new()
		{
			return _people.Select(
				p =>
				new T
				{
					LogOnBusinessUnitId = message.LogOnBusinessUnitId,
					LogOnDatasource = message.LogOnDatasource,
					PersonId = p.Id.GetValueOrDefault(),
					ScenarioId = _defaultScenario.Id.GetValueOrDefault(),
					Timestamp = DateTime.UtcNow,
					StartDateTime = _utcPeriod.StartDateTime,
					EndDateTime = _utcPeriod.EndDateTime,
					SkipDelete = true
				}).ToArray();
		}
	}
}