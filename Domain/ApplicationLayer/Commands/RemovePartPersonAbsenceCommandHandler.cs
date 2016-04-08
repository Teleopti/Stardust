using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePartPersonAbsenceCommandHandler : IHandleCommand<RemovePartPersonAbsenceCommand>
	{
		private readonly IPersonAbsenceRemover _personAbsenceRemover;

		public RemovePartPersonAbsenceCommandHandler(IPersonAbsenceRemover personAbsenceRemover)
		{
			_personAbsenceRemover = personAbsenceRemover;
		}

		public void Handle(RemovePartPersonAbsenceCommand command)
		{
			var personAbsences = command.PersonAbsences.ToList();
			if (!personAbsences.Any() || !personAbsences.Any(pa => pa.Period.Intersect(command.PeriodToRemove)))
			{
				return;
			}

			var person = command.Person;
			var errors =
				_personAbsenceRemover.RemovePartPersonAbsence(command.ScheduleDate, person, personAbsences,
					command.PeriodToRemove, command.TrackedCommandInfo).ToList();
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
