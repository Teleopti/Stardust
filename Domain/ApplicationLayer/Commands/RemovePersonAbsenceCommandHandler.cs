using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePersonAbsenceCommandHandler : IHandleCommand<RemovePersonAbsenceCommand>
	{
		private readonly IWriteSideRepository<IPersonAbsence> _personAbsenceRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly ISaveSchedulePartService _saveSchedulePartService;

		public RemovePersonAbsenceCommandHandler(IWriteSideRepository<IPersonAbsence> personAbsenceRepository,
			IScheduleRepository scheduleRepository,
			IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate,
			ISaveSchedulePartService saveSchedulePartService)
		{
			_personAbsenceRepository = personAbsenceRepository;
			_scheduleRepository = scheduleRepository;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_saveSchedulePartService = saveSchedulePartService;
		}

		public void Handle(RemovePersonAbsenceCommand command)
		{
			var personAbsence = (PersonAbsence)_personAbsenceRepository.LoadAggregate(command.PersonAbsenceId);
			personAbsence.RemovePersonAbsence(command.TrackedCommandInfo);
			removePersonAbsenceFromScheduleDay(personAbsence);
		}

		private void removePersonAbsenceFromScheduleDay(IPersonAbsence personAbsence)
		{
			var person = personAbsence.Person;
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var startDate = new DateOnly(personAbsence.Period.StartDateTimeLocal(timeZone));
			var endDate = startDate.AddDays(1);

			var scheduleDictionary =
					_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(
						person, new ScheduleDictionaryLoadOptions(false, false),
						new DateOnlyPeriod(startDate, endDate), personAbsence.Scenario);

			var scheduleRange = scheduleDictionary[person];
			var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);

			var scheduleDay = scheduleRange.ScheduledDay(startDate) as ExtractedSchedule;

			if (scheduleDay != null)
			{
				scheduleDay.Remove(personAbsence);
			}

			_saveSchedulePartService.Save(scheduleDay, rules, KeepOriginalScheduleTag.Instance);
		}
	}
}