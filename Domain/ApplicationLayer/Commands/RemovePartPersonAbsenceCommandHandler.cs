using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePartPersonAbsenceCommandHandler : IHandleCommand<RemovePartPersonAbsenceCommand>
	{
		private readonly IWriteSideRepository<IPersonAbsence> _personAbsenceRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly ISaveSchedulePartService _saveSchedulePartService;
		private readonly IPersonAbsenceCreator _personAbsenceCreator;

		public RemovePartPersonAbsenceCommandHandler(IWriteSideRepository<IPersonAbsence> personAbsenceRepository,
			IScheduleStorage scheduleStorage,
			IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate,
			ISaveSchedulePartService saveSchedulePartService,
			IPersonAbsenceCreator personAbsenceCreator)
		{
			_personAbsenceRepository = personAbsenceRepository;
			_scheduleStorage = scheduleStorage;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_saveSchedulePartService = saveSchedulePartService;
			_personAbsenceCreator = personAbsenceCreator;
		}

		public void Handle(RemovePartPersonAbsenceCommand command)
		{
			var personAbsence = (PersonAbsence) _personAbsenceRepository.LoadAggregate(command.PersonAbsenceId);
			if (personAbsence == null || shouldNotRemoveAbsence(personAbsence.Period, command.PeriodToRemove))
			{
				return;
			}
			personAbsence.RemovePersonAbsence(command.TrackedCommandInfo);
			removePartPersonAbsenceFromScheduleDay(personAbsence, command.PeriodToRemove, command.TrackedCommandInfo);
		}

		private static bool shouldNotRemoveAbsence(DateTimePeriod absencePeriod, DateTimePeriod periodToRemove)
		{
			return
				// Absence Period is later period to remove
				absencePeriod.StartDateTime > periodToRemove.EndDateTime
					// Absence Period is ealier than period to remove
				|| absencePeriod.EndDateTime < periodToRemove.StartDateTime;
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

		private static IList<DateTimePeriod> getPeriodsForNewAbsence(DateTimePeriod originalAbsencePeriod,
			DateTimePeriod periodToRemove)
		{
			var periods = new List<DateTimePeriod>();

			if (shouldNotRemoveAbsence(originalAbsencePeriod, periodToRemove)
				// Entire absence will be removed 
				|| (originalAbsencePeriod.StartDateTime > periodToRemove.StartDateTime &&
					originalAbsencePeriod.EndDateTime < periodToRemove.EndDateTime))
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
	}
}
