using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePartPersonAbsenceCommandHandler : IHandleCommand<RemovePartPersonAbsenceCommand>
	{
		private readonly IWriteSideRepository<IPersonAbsence> _personAbsenceRepository;
		private readonly IPersonAbsenceRemover _personAbsenceRemover;

		public RemovePartPersonAbsenceCommandHandler(IWriteSideRepository<IPersonAbsence> personAbsenceRepository,
			IPersonAbsenceRemover personAbsenceRemover)
		{
			_personAbsenceRepository = personAbsenceRepository;
			_personAbsenceRemover = personAbsenceRemover;
		}

		public void Handle(RemovePartPersonAbsenceCommand command)
		{
			foreach (var personAbsenceId in command.PersonAbsenceIds)
			{
				var personAbsence = (PersonAbsence) _personAbsenceRepository.LoadAggregate(personAbsenceId);
				if (personAbsence != null && personAbsence.Period.Intersect(command.PeriodToRemove))
				{
					_personAbsenceRemover.RemovePartPersonAbsence(personAbsence, command.PeriodToRemove, command.TrackedCommandInfo);
				}
			}
		}
	}
}
