using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePersonAbsenceCommandHandler : IHandleCommand<RemovePersonAbsenceCommand>
	{
		private readonly IPersonAbsenceRemover _personAbsenceRemover;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;

		public RemovePersonAbsenceCommandHandler(IPersonAbsenceRemover personAbsenceRemover, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario)
		{
			_personAbsenceRemover = personAbsenceRemover;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
		}

		public void Handle(RemovePersonAbsenceCommand command)
		{
			var personAbsences = command.PersonAbsence;
			if (personAbsences == null)
			{
				return;
			}

			var person = command.Person;
			var scheduleDate = new DateOnly (command.ScheduleDate);
			var endDate = scheduleDate.AddDays(1);

			var scheduleDictionary =
				_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false, false),
					new DateOnlyPeriod(scheduleDate, endDate), 
					_currentScenario.Current());

			var scheduleRange = scheduleDictionary[person];


			var errors =
				_personAbsenceRemover.RemovePersonAbsence(scheduleDate, person, command.PersonAbsence, scheduleRange, command.TrackedCommandInfo).ToList();
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