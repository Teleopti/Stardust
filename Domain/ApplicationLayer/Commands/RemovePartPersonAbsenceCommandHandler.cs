using System.Collections.Generic;
using System.Linq;
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
			var personAbsence = (PersonAbsence) _personAbsenceRepository.LoadAggregate(command.PersonAbsenceId);
			if (personAbsence == null || !personAbsence.Period.Intersect(command.PeriodToRemove))
			{
				return;
			}

			var errors =
				_personAbsenceRemover.RemovePartPersonAbsence(personAbsence, command.PeriodToRemove, command.TrackedCommandInfo)
					.ToList();
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
