using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePersonAbsenceCommandHandler : IHandleCommand<RemovePersonAbsenceCommand>
	{
		private readonly IPersonAbsenceRemover _personAbsenceRemover;

		public RemovePersonAbsenceCommandHandler(IPersonAbsenceRemover personAbsenceRemover)
		{
			_personAbsenceRemover = personAbsenceRemover;
		}

		public void Handle(RemovePersonAbsenceCommand command)
		{
			var personAbsences = command.PersonAbsences;
			if (personAbsences == null || !personAbsences.Any())
			{
				return;
			}

			var person = command.Person;
			var errors =
				_personAbsenceRemover.RemovePersonAbsence(command.ScheduleDate, person, command.PersonAbsences,
				command.TrackedCommandInfo).ToList();
			if (!errors.Any())
			{
				return;
			}

			command.Errors = new ActionErrorMessage
			{
				PersonId = person.Id.GetValueOrDefault(),
				PersonName = person.Name,
				ErrorMessages = errors
			};
		}
	}
}