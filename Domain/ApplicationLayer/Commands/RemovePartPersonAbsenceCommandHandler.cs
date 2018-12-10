using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePartPersonAbsenceCommandHandler : IHandleCommand<RemovePartPersonAbsenceCommand>
	{
		private readonly IPersonAbsenceRemover _personAbsenceRemover;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;

		public RemovePartPersonAbsenceCommandHandler(IPersonAbsenceRemover personAbsenceRemover, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario)
		{
			_personAbsenceRemover = personAbsenceRemover;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
		}

		public void Handle(RemovePartPersonAbsenceCommand command)
		{
			var personAbsences = command.PersonAbsence;
			if (personAbsences == null || !personAbsences.Period.Intersect(command.PeriodToRemove))
			{
				return;
			}

			var person = command.Person;
			var scheduleDate = new DateOnly(command.ScheduleDate);
			var endDate = scheduleDate.AddDays(1);

			var scheduleDictionary =
				_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false, false),
					new DateOnlyPeriod(scheduleDate, endDate), 
					_currentScenario.Current());

			var scheduleRange = scheduleDictionary[person];
			var errors =
				_personAbsenceRemover.RemovePartPersonAbsence(scheduleDate, person, personAbsences,
					command.PeriodToRemove, scheduleRange, command.TrackedCommandInfo).ToList();
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
