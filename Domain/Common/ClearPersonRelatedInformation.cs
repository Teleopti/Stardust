using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Common
{
	public class ClearPersonRelatedInformation : IPersonLeavingUpdater
	{
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;

		public ClearPersonRelatedInformation(IPersonAssignmentRepository personAssignmentRepository, IScenarioRepository scenarioRepository, IPersonAbsenceRepository personAbsenceRepository)
		{
			_personAssignmentRepository = personAssignmentRepository;
			_scenarioRepository = scenarioRepository;
			_personAbsenceRepository = personAbsenceRepository;
		}

		public void Execute(DateOnly leavingDate, IPerson person)
		{
			var period = new DateOnlyPeriod(leavingDate.AddDays(1), DateOnly.MaxValue);
			var dateTimePeriod = period.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());

			foreach (var scenario in _scenarioRepository.LoadAll())
			{
				var assignmentsToRemove = _personAssignmentRepository.Find(new[] {person}, period, scenario);
				foreach (var personAssignment in assignmentsToRemove)
				{
					_personAssignmentRepository.Remove(personAssignment);
				}

				var absencesToRemove = _personAbsenceRepository.Find(new[] {person},
					dateTimePeriod, scenario);
				foreach (var personAbsence in absencesToRemove)
				{
					if (personAbsence.Period.StartDateTime < dateTimePeriod.StartDateTime)
					{
						continue;
					}
					((IRepository<IPersonAbsence>) _personAbsenceRepository).Remove(personAbsence);
				}
			}

			person.RemoveAllPeriodsAfter(leavingDate);
		}
	}
}