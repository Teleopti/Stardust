using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class PersonAbsenceRemover : IPersonAbsenceRemover
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(PersonAbsenceRemover));

		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly ISaveSchedulePartService _saveSchedulePartService;
		private readonly IPersonAbsenceCreator _personAbsenceCreator;
		private readonly ICheckingPersonalAccountDaysProvider _checkingPersonalAccountDaysProvider;
		private readonly ICurrentAuthorization _currentAuthorization;

		public PersonAbsenceRemover(IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate,
			ISaveSchedulePartService saveSchedulePartService,
			IPersonAbsenceCreator personAbsenceCreator,
		 ICheckingPersonalAccountDaysProvider checkingPersonalAccountDaysProvider, ICurrentAuthorization currentAuthorization)
		{
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_saveSchedulePartService = saveSchedulePartService;
			_personAbsenceCreator = personAbsenceCreator;
			_checkingPersonalAccountDaysProvider = checkingPersonalAccountDaysProvider;
			_currentAuthorization = currentAuthorization;
		}

		public IEnumerable<string> RemovePersonAbsence(DateOnly scheduleDate, IPerson person,
			IPersonAbsence personAbsence, IScheduleRange scheduleRange, TrackedCommandInfo commandInfo = null)
		{
			var errors = removePersonAbsenceFromScheduleDay(scheduleDate, person, personAbsence, commandInfo,
				scheduleRange);
			return errors ?? new List<string>();
		}

		public IEnumerable<string> RemovePartPersonAbsence(DateOnly scheduleDate, IPerson person,
			IPersonAbsence personAbsence, DateTimePeriod periodToRemove, IScheduleRange scheduleRange,
			TrackedCommandInfo commandInfo = null)
		{
			var errors = removePersonAbsenceFromScheduleDay(scheduleDate, person, personAbsence, commandInfo,
				scheduleRange, periodToRemove);
			return errors ?? new List<string>();
		}

		private bool canRemovePersonAbsence(IPerson person, DateOnly startDate)
		{
			return !person.PersonWriteProtection.IsWriteProtected(startDate) || _currentAuthorization.Current()
					   .IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule);
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

		private IEnumerable<string> removePersonAbsenceFromScheduleDay(
			DateOnly scheduleDate, IPerson person, IPersonAbsence personAbsence,
			TrackedCommandInfo commandInfo, IScheduleRange scheduleRange,
			DateTimePeriod? periodToRemove = null)
		{
			var errorMessages = new List<string>();

			if (!canRemovePersonAbsence(person, scheduleDate))
			{
				return new[] { Resources.CouldNotRemoveAbsenceFromProtectedSchedule };
			}

			var scheduleDay = scheduleRange.ScheduledDay(scheduleDate, true) as ExtractedSchedule;

			var isFromMidnight = new DateOnly(personAbsence.Period.StartDateTime) != scheduleDate;
			if (isFromMidnight)
			{
				var personAssignment = scheduleDay.PersonAssignment();
				var period = personAssignment == null || !personAssignment.ShiftLayers.Any() ? scheduleDay.Period : personAssignment.Period;
				var eventPeriod = new DateTimePeriod(period.StartDateTime, personAbsence.Period.EndDateTime);
				personAbsence.RemovePersonAbsence(commandInfo, eventPeriod);
			}
			else
			{
				personAbsence.RemovePersonAbsence(commandInfo, personAbsence.Period);
			}

			if (scheduleDay != null)
			{
				errorMessages = removePersonAbsenceFromScheduleDay(scheduleDate, person, personAbsence, scheduleRange, scheduleDay);
			}
			else
			{
				logger.Error($"Only logged for cancelling absence request used to reproduce bug#79030 PersonAbsenceRemover:{scheduleDate}| {personAbsence.Id}");
			}

			if (errorMessages.Any())
			{
				return errorMessages;
			}

			if (!periodToRemove.HasValue)
				return errorMessages;

			var newAbsencePeriods = getPeriodsForNewAbsence(personAbsence.Period, periodToRemove.Value);
			if (newAbsencePeriods.Any())
			{
				errorMessages.AddRange(createNewAbsencesForSplitAbsence(person, personAbsence,
					newAbsencePeriods, commandInfo, scheduleDay, scheduleRange).ToList());
			}


			return errorMessages;
		}

		private List<string> removePersonAbsenceFromScheduleDay(DateOnly scheduleDate, IPerson person, IPersonAbsence personAbsence,
			IScheduleRange scheduleRange, ExtractedSchedule scheduleDay)
		{
			var scheduleDays = new List<IScheduleDay> { scheduleDay };
			// To be more efficient, we only return the first schedule day for each personal account for NewPersonAccountRule
			scheduleDays.AddRange(
				getScheduleDaysForCheckingAccount(personAbsence, scheduleDate, scheduleRange, person));

			scheduleDays.ForEach(s => s.Remove(personAbsence));

			var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);
			var errorMessages = _saveSchedulePartService.Save(scheduleDays, rules, KeepOriginalScheduleTag.Instance);
			return errorMessages != null && errorMessages.Any() ? errorMessages.ToList() : new List<string>();
		}

		private IEnumerable<string> createNewAbsencesForSplitAbsence(IPerson person,
			IPersonAbsence personAbsence, IEnumerable<DateTimePeriod> newAbsencePeriods,
			TrackedCommandInfo commandInfo, IScheduleDay scheduleDay, IScheduleRange scheduleRange)
		{
			var errorMessages = new List<string>();

			foreach (var period in newAbsencePeriods)
			{
				// xinfli: The second parameter "isFullDayAbsence" doesn't matter, since it just raise different event
				// and all the events will be converted to "ScheduleChangedEvent" (Refer to ScheduleChangedEventPublisher class)
				var errors = _personAbsenceCreator.Create(new AbsenceCreatorInfo
				{
					Person = person,
					Absence = personAbsence.Layer.Payload,
					ScheduleDay = scheduleDay,
					ScheduleRange = scheduleRange,
					AbsenceTimePeriod = period,
					TrackedCommandInfo = commandInfo
				}, false);

				if (errors == null || !errors.Any()) continue;

				errorMessages.AddRange(errors);
			}

			return errorMessages;
		}

		private IEnumerable<IScheduleDay> getScheduleDaysForCheckingAccount(IPersonAbsence personAbsence,
			DateOnly startDate,
			IScheduleRange scheduleRange, IPerson person)
		{
			var daysForChecking =
					   _checkingPersonalAccountDaysProvider.GetDays(personAbsence.Layer.Payload, person,
						   personAbsence.Period);
			if (daysForChecking.DayCount() <= 1) return Enumerable.Empty<IScheduleDay>();

			var periodToLoad = daysForChecking.StartDate == startDate
				? new DateOnlyPeriod(daysForChecking.StartDate.AddDays(1), daysForChecking.EndDate)
				: daysForChecking;

			return scheduleRange.ScheduledDayCollection(periodToLoad);
		}
	}
}