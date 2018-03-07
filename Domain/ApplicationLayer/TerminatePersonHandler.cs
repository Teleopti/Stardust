using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class TerminatePersonHandler:IHandleEvent<PersonTerminalDateChangedEvent>, IRunOnHangfire

	{
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;

		public TerminatePersonHandler(IPersonRepository personRepository, IScenarioRepository scenarioRepository,
			IPersonAssignmentRepository personAssignmentRepository, IPersonAbsenceRepository personAbsenceRepository)
		{
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_personAssignmentRepository = personAssignmentRepository;
			_personAbsenceRepository = personAbsenceRepository;
		}

		public void Handle(PersonTerminalDateChangedEvent @event)
		{
			if(!@event.TerminationDate.HasValue)
				return;
			var leavingDate = new DateOnly(@event.TerminationDate.Value);
			var person = _personRepository.Get(@event.PersonId);
			var period = new DateOnlyPeriod(leavingDate.AddDays(1), DateOnly.MaxValue);
			var dateTimePeriod = period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());

			foreach (var scenario in _scenarioRepository.LoadAll())
			{
				var assignmentsToRemove = _personAssignmentRepository.Find(new[] { person }, period, scenario);
				foreach (var personAssignment in assignmentsToRemove)
				{
					_personAssignmentRepository.Remove(personAssignment);
				}

				var absencesToRemove = _personAbsenceRepository.Find(new[] { person },
					dateTimePeriod, scenario);
				foreach (var personAbsence in absencesToRemove)
				{
					if (personAbsence.Period.StartDateTime < dateTimePeriod.StartDateTime)
					{
						continue;
					}
					((IRepository<IPersonAbsence>)_personAbsenceRepository).Remove(personAbsence);
				}
			}
			
			person.RemoveAllPeriodsAfter(leavingDate);
		}
	}
}
