using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class MyTeamRemovePersonAbsenceCommandHandler : IHandleCommand<MyTeamRemovePersonAbsenceCommand>
	{
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly ISaveSchedulePartService _saveSchedulePartService;

		public MyTeamRemovePersonAbsenceCommandHandler(
			IPersonAbsenceRepository personAbsenceRepository,
			IScheduleStorage scheduleStorage,
			IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate,
			ISaveSchedulePartService saveSchedulePartService)
		{
			_personAbsenceRepository = personAbsenceRepository;
			_scheduleStorage = scheduleStorage;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_saveSchedulePartService = saveSchedulePartService;
		}

		public void Handle(MyTeamRemovePersonAbsenceCommand command)
		{
			var personAbsence = (PersonAbsence)_personAbsenceRepository.LoadAggregate(command.PersonAbsenceId);
			if (personAbsence == null)
			{
				return;
			}

			personAbsence.RemovePersonAbsence(command.TrackedCommandInfo);

			var person = personAbsence.Person;
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var startDate = new DateOnly(personAbsence.Period.StartDateTimeLocal(timeZone));
			var endDate = startDate.AddDays(1);

			var scheduleDictionary =
				_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
					person, new ScheduleDictionaryLoadOptions(false, false),
					new DateOnlyPeriod(startDate, endDate), personAbsence.Scenario);

			var scheduleRange = scheduleDictionary[person];
			var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);

			var scheduleDay = scheduleRange.ScheduledDay(startDate) as ExtractedSchedule;
			if (scheduleDay != null) scheduleDay.Remove(personAbsence);

			_saveSchedulePartService.Save(scheduleDay, rules, KeepOriginalScheduleTag.Instance);
		}
	}
}