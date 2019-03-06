using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[EnabledBy(Toggles.WFM_Clear_Data_After_Leaving_Date_47768)]
	public class TerminatePersonHandler:IHandleEvent<PersonTerminalDateChangedEvent>, IRunOnHangfire
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IStudentAvailabilityDayRepository _studentAvailabilityDayRepository;
		private readonly IPreferenceDayRepository _preferenceDayRepository;

		public TerminatePersonHandler(IPersonRepository personRepository, IScenarioRepository scenarioRepository,
			IPersonAssignmentRepository personAssignmentRepository, IPersonAbsenceRepository personAbsenceRepository,
			IEventPopulatingPublisher eventPublisher, IStudentAvailabilityDayRepository studentAvailabilityDayRepository,
			IPreferenceDayRepository preferenceDayRepository)
		{
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_personAssignmentRepository = personAssignmentRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_eventPublisher = eventPublisher;
			_studentAvailabilityDayRepository = studentAvailabilityDayRepository;
			_preferenceDayRepository = preferenceDayRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(PersonTerminalDateChangedEvent @event)
		{
			if(!@event.TerminationDate.HasValue)
				return;
			var leavingDate = new DateOnly(@event.TerminationDate.Value);
			var person = _personRepository.Get(@event.PersonId);
			var personList = new[] {person};
			var period = new DateOnlyPeriod(leavingDate.AddDays(1), DateOnly.MaxValue);
			var dateTimePeriod = period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var events = new List<IEvent>();

			var scenarios = _scenarioRepository.LoadAll();
			foreach (var scenario in scenarios)
			{
				var scheduleEvent  = createScheduleChangeEvent(@event, personList, period, scenario, dateTimePeriod);
				if(scheduleEvent != null)
					events.Add(scheduleEvent);
			}

			var availabilityEvent = createAvailabilityChangeEvent(@event, period, personList);
			if(availabilityEvent != null)
				events.Add(availabilityEvent);

			var preferenceEvent = createPreferenceDeletedEvent(period, person);
			if(preferenceEvent != null)
				events.Add(preferenceEvent);

			if(events.Any())
				_eventPublisher.Publish(events.ToArray());
		}

		private IEvent createPreferenceDeletedEvent(DateOnlyPeriod period, IPerson person)
		{
			var preferenceDays = _preferenceDayRepository.Find(period, person).ToList();

			var dates = new List<DateTime>();

			foreach (var preferenceDay in preferenceDays)
			{
				_preferenceDayRepository.Remove(preferenceDay);
				dates.Add(preferenceDay.RestrictionDate.Date);
			}

			if (dates.Any())
			{
				return new PreferenceDeletedEvent
				{
					PersonId = person.Id.Value,
					RestrictionDates = dates
				};
			}

			return null;
		}

		private IEvent createAvailabilityChangeEvent(PersonTerminalDateChangedEvent @event, DateOnlyPeriod period,
			IPerson[] personList)
		{
			var studentAvailabilityDays = _studentAvailabilityDayRepository.Find(period, personList);
			foreach (var availabilityDay in studentAvailabilityDays)
			{
				_studentAvailabilityDayRepository.Remove(availabilityDay);
			}

			if (studentAvailabilityDays.Any())
			{
				return new AvailabilityChangedEvent()
				{
					PersonId = @event.PersonId,
					Dates = studentAvailabilityDays.Select(a => a.RestrictionDate).ToList()
				};
			}

			return null;
		}

		private IEvent createScheduleChangeEvent(PersonTerminalDateChangedEvent @event, IPerson[] personList,
			DateOnlyPeriod period, IScenario scenario, DateTimePeriod dateTimePeriod)
		{
			var assignmentsToRemove = _personAssignmentRepository.Find(personList, period, scenario);
			var allPeriods = new List<DateTimePeriod>();
			foreach (var personAssignment in assignmentsToRemove)
			{
				_personAssignmentRepository.Remove(personAssignment);
				allPeriods.Add(personAssignment.Period);
			}

			var absencesToRemove = _personAbsenceRepository.Find(personList, dateTimePeriod, scenario);
			foreach (var personAbsence in absencesToRemove)
			{
				if (personAbsence.Period.StartDateTime < dateTimePeriod.StartDateTime)
				{
					continue;
				}

				((IRepository<IPersonAbsence>) _personAbsenceRepository).Remove(personAbsence);
				allPeriods.Add(personAbsence.Period);
			}

			if (!allPeriods.Any())
			{
				return null;
			}

			var periodStart = allPeriods.Select(x => x.StartDateTime).Min();
			var periodEnd = allPeriods.Select(x => x.EndDateTime).Max();

			return new ScheduleChangedEvent
			{
				LogOnBusinessUnitId = scenario.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault(),
				ScenarioId = scenario.Id.Value,
				PersonId = @event.PersonId,
				StartDateTime = periodStart,
				EndDateTime = periodEnd
			};
		}
	}
}
