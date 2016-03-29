using System.Collections.Generic;
using System.Linq;
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
		private readonly IPersonAbsenceCreator _personAbsenceCreator;

		public PersonAbsenceRemover(
			IScheduleStorage scheduleStorage,
			IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate,
			ISaveSchedulePartService saveSchedulePartService,
			IPersonAbsenceCreator personAbsenceCreator)
		{
			_scheduleStorage = scheduleStorage;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_saveSchedulePartService = saveSchedulePartService;
			_personAbsenceCreator = personAbsenceCreator;
		}
		
		public void RemovePersonAbsence(IPersonAbsence personAbsence, TrackedCommandInfo commandInfo = null)
		{
			personAbsence.RemovePersonAbsence(commandInfo);
			removePersonAbsenceFromScheduleDay(personAbsence);
		}

		public void RemovePartPersonAbsence(IPersonAbsence personAbsence, DateTimePeriod periodToRemove,
			TrackedCommandInfo commandInfo = null)
		{
			personAbsence.RemovePersonAbsence(commandInfo);
			removePartPersonAbsenceFromScheduleDay(personAbsence, periodToRemove, commandInfo);
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

		private static IList<DateTimePeriod> getPeriodsForNewAbsence(DateTimePeriod originalAbsencePeriod,
			DateTimePeriod periodToRemove)
		{
			var periods = new List<DateTimePeriod>();

			if (!originalAbsencePeriod.Intersect(periodToRemove)
				// Entire absence will be removed 
				|| periodToRemove.Contains(originalAbsencePeriod))
			{
				return periods;
			}

			if (originalAbsencePeriod.StartDateTime < periodToRemove.StartDateTime)
			{
				periods.Add(new DateTimePeriod(originalAbsencePeriod.StartDateTime, periodToRemove.StartDateTime));
			}

			if (originalAbsencePeriod.EndDateTime > periodToRemove.EndDateTime)
			{
				periods.Add(new DateTimePeriod(periodToRemove.EndDateTime, originalAbsencePeriod.EndDateTime));
			}

			return periods;
		}

		private void removePartPersonAbsenceFromScheduleDay(IPersonAbsence personAbsence, DateTimePeriod periodToRemove,
			TrackedCommandInfo trackedCommandInfo)
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

			var newAbsencePeriods = getPeriodsForNewAbsence(personAbsence.Period, periodToRemove);
			if (!newAbsencePeriods.Any()) return;

			foreach (var period in newAbsencePeriods)
			{
				// xinfli: The second parameter "isFullDayAbsence" doesn't matter, since it just raise different event
				// and all the events will be converted to "ScheduleChangedEvent" (Refer to ScheduleChangedEventPublisher class)
				_personAbsenceCreator.Create(new AbsenceCreatorInfo
				{
					Person = person,
					Absence = personAbsence.Layer.Payload,
					ScheduleDay = scheduleDay,
					ScheduleRange = scheduleRange,
					AbsenceTimePeriod = period,
					TrackedCommandInfo = trackedCommandInfo
				}, false);
			}
		}
	}
}