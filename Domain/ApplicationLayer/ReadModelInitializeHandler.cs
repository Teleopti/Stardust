using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IEventPublisher _eventPublisher;
		
		public ReadModelInitializeHandler(IPersonRepository personRepository,
			IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister,
			IScheduleDayReadModelRepository scheduleDayReadModelRepository,
			IPersonScheduleDayReadModelPersister personScheduleDayReadModelRepository,
			ICurrentScenario scenarioRepository, IEventPublisher eventPublisher,
			IDistributedLockAcquirer distributedLockAcquirer,
			IPersonAssignmentRepository personAssignmentRepository,
			IPersonAbsenceRepository personAbsenceRepository)
		{
			_personRepository = personRepository;
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_scenarioRepository = scenarioRepository;
			_eventPublisher = eventPublisher;
			_distributedLockAcquirer = distributedLockAcquirer;
			_personAssignmentRepository = personAssignmentRepository;
			_personAbsenceRepository = personAbsenceRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(InitialLoadScheduleProjectionEvent @event)
		{
			var businessUnitId = @event.LogOnBusinessUnitId;
			_distributedLockAcquirer.TryLockForTypeOfAnd(this, businessUnitId.ToString(), () =>
			// Use a lock for each business unit to only run this once for each BU
			{
				var utcNow = DateTime.UtcNow;
				var startDate = utcNow.Date.AddDays(@event.StartDays);
				var endDate = utcNow.Date.AddDays(@event.EndDays);
				var period = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
				var utcPeriod = period.ToDateTimePeriod(TimeZoneInfo.Utc);
				if (!anyAgentScheduled(businessUnitId, period))
				{
					return;
				}
				
				var projectionModelInitialized = _scheduleProjectionReadOnlyPersister.IsInitialized();
				var scheduleDayModelInitialized = _scheduleDayReadModelRepository.IsInitialized();
				var personScheduleDayModelInitialized = _personScheduleDayReadModelRepository.IsInitialized();
				if (projectionModelInitialized && scheduleDayModelInitialized && personScheduleDayModelInitialized)
				{
					return;
				}

				var people = _personRepository.LoadAll()
					.Where(p => p.PersonPeriodCollection.Any())
					.ToList();
				var defaultScenario = _scenarioRepository.Current();

				var messages = new List<ScheduleChangedEventBase>();
				if (!projectionModelInitialized && !scheduleDayModelInitialized && !personScheduleDayModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleChangedEvent>(@event, people, defaultScenario, utcPeriod));

					projectionModelInitialized = true;
					scheduleDayModelInitialized = true;
					personScheduleDayModelInitialized = true;
				}

				if (!projectionModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleInitializeTriggeredEventForScheduleProjection>(@event, people, defaultScenario, utcPeriod));
				}

				if (!scheduleDayModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleInitializeTriggeredEventForScheduleDay>(@event, people, defaultScenario, utcPeriod));
				}

				if (!personScheduleDayModelInitialized)
				{
					messages.AddRange(initialLoad<ScheduleInitializeTriggeredEventForPersonScheduleDay>(@event, people, defaultScenario, utcPeriod));
				}

				messages.ForEach(m => _eventPublisher.Publish(m));
			});
		}

		private bool anyAgentScheduled(Guid businessUnitId, DateOnlyPeriod period)
		{
			var existsAnyPersonAssignment = _personAssignmentRepository.IsThereScheduledAgents(businessUnitId, period);
			var existsAnyPersonAbsence = _personAbsenceRepository.IsThereScheduledAgents(businessUnitId, period);
			return existsAnyPersonAssignment || existsAnyPersonAbsence;
		}

		private IEnumerable<T> initialLoad<T>(InitialLoadScheduleProjectionEvent message, IEnumerable<IPerson> people,
			IScenario defaultScenario, DateTimePeriod utcPeriod) where T : ScheduleChangedEventBase, new()
		{
			return people.Select(
				p =>
				new T
				{
					LogOnBusinessUnitId = message.LogOnBusinessUnitId,
					LogOnDatasource = message.LogOnDatasource,
					PersonId = p.Id.GetValueOrDefault(),
					ScenarioId = defaultScenario.Id.GetValueOrDefault(),
					Timestamp = DateTime.UtcNow,
					StartDateTime = utcPeriod.StartDateTime,
					EndDateTime = utcPeriod.EndDateTime,
					SkipDelete = true
				}).ToArray();
		}
	}
}