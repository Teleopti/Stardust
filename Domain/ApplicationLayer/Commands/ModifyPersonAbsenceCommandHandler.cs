using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ModifyPersonAbsenceCommandHandler : IHandleCommand<ModifyPersonAbsenceCommand>
	{
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IUserTimeZone _timeZone;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly ISaveSchedulePartService _saveSchedulePartService;

		public ModifyPersonAbsenceCommandHandler(IPersonAbsenceRepository personAbsenceRepository,
			IUserTimeZone timeZone,
			IScheduleStorage scheduleStorage,
			IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate,
			ISaveSchedulePartService saveSchedulePartService)
		{
			_personAbsenceRepository = personAbsenceRepository;
			_timeZone = timeZone;
			_scheduleStorage = scheduleStorage;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_saveSchedulePartService = saveSchedulePartService;
		}

		public void Handle(ModifyPersonAbsenceCommand command)
		{
			
			var personAbsence = (PersonAbsence)_personAbsenceRepository.Get(command.PersonAbsenceId);
			if (personAbsence == null)
				throw new InvalidOperationException(
					$"Person Absence is not found. StartTime: {command.StartTime} EndTime: {command.EndTime} PersonId: {command.PersonId} AbsenceId: {command.PersonAbsenceId}  ");

			var person = personAbsence.Person;
			var absenceTimePeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartTime, _timeZone.TimeZone()),
													   TimeZoneHelper.ConvertToUtc(command.EndTime, _timeZone.TimeZone()));

			var startDate = new DateOnly(personAbsence.Period.StartDateTimeLocal(_timeZone.TimeZone()));
			var endDate = new DateOnly(personAbsence.Period.EndDateTimeLocal(_timeZone.TimeZone()));
			var existingPeriodOfAbsence = new DateOnlyPeriod (startDate, endDate);

			var scheduleDictionary =
					_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
						person, new ScheduleDictionaryLoadOptions(false, false),
						existingPeriodOfAbsence, personAbsence.Scenario);

			var scheduleRange = scheduleDictionary[person];
			var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);

			var scheduleDaysToUpdate =
				from day in existingPeriodOfAbsence.DayCollection()
				select scheduleRange.ScheduledDay (startDate) as ExtractedSchedule;

			personAbsence.ModifyPersonAbsencePeriod(absenceTimePeriod, command.TrackedCommandInfo);

			foreach (var scheduleDay in scheduleDaysToUpdate)
			{
				_saveSchedulePartService.Save(scheduleDay, rules, KeepOriginalScheduleTag.Instance);
			}

		}
	}
}
