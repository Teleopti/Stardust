using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePersonAbsenceCommandHandler : IHandleCommand<RemovePersonAbsenceCommand>
	{
		private readonly IWriteSideRepository<IPersonAbsence> _personAbsenceRepository;
		private readonly IPersonAbsenceRemover _personAbsenceRemover;

		public RemovePersonAbsenceCommandHandler(IWriteSideRepository<IPersonAbsence> personAbsenceRepository, IPersonAbsenceRemover personAbsenceRemover)
		{
			_personAbsenceRepository = personAbsenceRepository;
			_personAbsenceRemover = personAbsenceRemover;
		}

		public void Handle(RemovePersonAbsenceCommand command)
		{
			foreach (var personAbsenceId in command.PersonAbsenceIds)
			{
				var personAbsence = (PersonAbsence) _personAbsenceRepository.LoadAggregate(personAbsenceId);
				if (personAbsence != null)
				{
					_personAbsenceRemover.RemovePersonAbsence(personAbsence, command.TrackedCommandInfo);
				}
			}
		}
	}
}
