using System.Linq;
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
			var personAbsence = (PersonAbsence) _personAbsenceRepository.LoadAggregate(command.PersonAbsenceId);
			if (personAbsence == null)
			{
				return;
			}

			var errors = _personAbsenceRemover.RemovePersonAbsence (personAbsence, command.TrackedCommandInfo).ToList();
			if (!errors.Any())
			{
				return;
			}

			command.Errors = new ActionErrorMessage
			{
				PersonId = personAbsence.Person.Id.GetValueOrDefault(),
				PersonName = personAbsence.Person.Name,
				ErrorMessages = errors
			};
		}
	}
}