using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemoveSelectedPersonAbsenceCommandHandler :IHandleCommand<RemoveSelectedPersonAbsenceCommand>
	{
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _personRepository;
		private readonly IPersonAbsenceRemover _personAbsenceRemover;

		public RemoveSelectedPersonAbsenceCommandHandler(ICurrentScenario currentScenario, IPersonAbsenceRepository personAbsenceRepository, IScheduleStorage scheduleStorage, IPersonRepository personRepository, IPersonAbsenceRemover personAbsenceRemover)
		{
			_currentScenario = currentScenario;
			_personAbsenceRepository = personAbsenceRepository;
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_personAbsenceRemover = personAbsenceRemover;
		}
		
		public void Handle(RemoveSelectedPersonAbsenceCommand command)
		{
			var scenario = _currentScenario.Current();
			var person = _personRepository.Get(command.PersonId);
			var personAbsences = _personAbsenceRepository.Get(command.PersonAbsenceId);
			
			var period = command.Date.ToDateOnlyPeriod().Inflate(1);

			var dictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
					person,
					new ScheduleDictionaryLoadOptions(false,false),
					period,
					scenario);

			var scheduleRange = dictionary[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);			
			var personAssignment = scheduleDay.PersonAssignment();

			var periodToMove = personAssignment != null && personAssignment.ShiftLayers.Any()? 
				personAssignment.Period :
				scheduleDay.Period;

			command.ErrorMessages = _personAbsenceRemover.RemovePartPersonAbsence(command.Date, person,personAbsences, periodToMove,scheduleRange, command.TrackedCommandInfo).ToList() ;
		}
	}
}
