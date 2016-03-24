﻿using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class PersonAbsenceRemover : IPersonAbsenceRemover
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly ISaveSchedulePartService _saveSchedulePartService;

		public PersonAbsenceRemover(
			IScheduleStorage scheduleStorage,
			IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate,
			ISaveSchedulePartService saveSchedulePartService)
		{
			_scheduleStorage = scheduleStorage;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_saveSchedulePartService = saveSchedulePartService;
		}
		
		public void RemovePersonAbsence(IPersonAbsence personAbsence, TrackedCommandInfo commandInfo = null)
		{
			personAbsence.RemovePersonAbsence(commandInfo);
			removePersonAbsenceFromScheduleDay(personAbsence);
		}

		private void removePersonAbsenceFromScheduleDay(IPersonAbsence personAbsence)
		{
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

			if (scheduleDay != null)
			{
				scheduleDay.Remove(personAbsence);
			}

			_saveSchedulePartService.Save(scheduleDay, rules, KeepOriginalScheduleTag.Instance);
		}
	}

	
}