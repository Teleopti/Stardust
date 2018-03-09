using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

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

		public TerminatePersonHandler(IPersonRepository personRepository, IScenarioRepository scenarioRepository,
			IPersonAssignmentRepository personAssignmentRepository, IPersonAbsenceRepository personAbsenceRepository,
			IEventPopulatingPublisher eventPublisher)
		{
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_personAssignmentRepository = personAssignmentRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_eventPublisher = eventPublisher;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(PersonTerminalDateChangedEvent @event)
		{
			if(!@event.TerminationDate.HasValue)
				return;
			var leavingDate = new DateOnly(@event.TerminationDate.Value);
			var person = _personRepository.Get(@event.PersonId);
			var period = new DateOnlyPeriod(leavingDate.AddDays(1), DateOnly.MaxValue);
			var dateTimePeriod = period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var events = new List<IEvent>();

			var scenarios = _scenarioRepository.LoadAll();
			foreach (var scenario in scenarios)
			{
				var assignmentsToRemove = _personAssignmentRepository.Find(new[] { person }, period, scenario);
				var allPeriods = new List<DateTimePeriod>();
				foreach (var personAssignment in assignmentsToRemove)
				{
					_personAssignmentRepository.Remove(personAssignment);
					allPeriods.Add(personAssignment.Period);
				}

				var absencesToRemove = _personAbsenceRepository.Find(new[] { person }, dateTimePeriod, scenario);
				foreach (var personAbsence in absencesToRemove)
				{
					if (personAbsence.Period.StartDateTime < dateTimePeriod.StartDateTime)
					{
						continue;
					}
					((IRepository<IPersonAbsence>)_personAbsenceRepository).Remove(personAbsence);
					allPeriods.Add(personAbsence.Period);
				}

				if (!allPeriods.Any())
				{
					continue;
				}

				var periodStart = allPeriods.Select(x => x.StartDateTime).Min();
				var periodEnd = allPeriods.Select(x => x.EndDateTime).Max();
				
				events.Add(new ScheduleChangedEvent
				{
					LogOnBusinessUnitId = scenario.BusinessUnit.Id.GetValueOrDefault(),
					ScenarioId = scenario.Id.Value,
					PersonId = @event.PersonId,
					StartDateTime = periodStart,
					EndDateTime = periodEnd
				});
			}

			//Ask anders
			//person.RemoveAllPeriodsAfter(leavingDate);

			_eventPublisher.Publish(events.ToArray());
		}
	}
}
