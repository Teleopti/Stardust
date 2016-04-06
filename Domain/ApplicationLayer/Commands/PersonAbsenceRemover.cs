using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class PersonAbsenceRemover : IPersonAbsenceRemover
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly ISaveSchedulePartService _saveSchedulePartService;
		private readonly IPersonAbsenceCreator _personAbsenceCreator;
		private readonly ILoggedOnUser _loggedOnUser;

		public PersonAbsenceRemover(
			IScheduleStorage scheduleStorage,
			IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate,
			ISaveSchedulePartService saveSchedulePartService,
			IPersonAbsenceCreator personAbsenceCreator,
			ILoggedOnUser loggedOnUser)
		{
			_scheduleStorage = scheduleStorage;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_saveSchedulePartService = saveSchedulePartService;
			_personAbsenceCreator = personAbsenceCreator;
			_loggedOnUser = loggedOnUser;
		}

		public IEnumerable<string> RemovePersonAbsence(IPersonAbsence personAbsence, TrackedCommandInfo commandInfo = null)
		{
			personAbsence.RemovePersonAbsence(commandInfo);
			var errorMessages = removePersonAbsenceFromScheduleDay(personAbsence);
			return errorMessages ?? new List<string>();
		}

		public IEnumerable<string> RemovePartPersonAbsence(IPersonAbsence personAbsence, DateTimePeriod periodToRemove,
			TrackedCommandInfo commandInfo = null)
		{
			personAbsence.RemovePersonAbsence(commandInfo);
			var errorMessages = removePartPersonAbsenceFromScheduleDay(personAbsence, periodToRemove, commandInfo);
			return errorMessages ?? new List<string>();
		}

		private bool canRemovePersonAbsence(IPerson person, DateOnly startDate)
		{
			var factory = new DefinedRaptorApplicationFunctionFactory();
			var functionPath = ApplicationFunction.FindByPath(factory.ApplicationFunctions,
				DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule);
			var functionPathAll = ApplicationFunction.FindByPath(factory.ApplicationFunctions,
				DefinedRaptorApplicationFunctionPaths.All);
			var currentUserRoles = _loggedOnUser.CurrentUser().PermissionInformation.ApplicationRoleCollection;
			var canModifyProtectedSchedule =
				currentUserRoles.Any(role => (role.ApplicationFunctionCollection.Contains(functionPath)
											  || role.ApplicationFunctionCollection.Contains(functionPathAll)));
			return !person.PersonWriteProtection.IsWriteProtected(startDate) || canModifyProtectedSchedule;
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

		private IEnumerable<string> removePersonAbsenceFromScheduleDay(IPersonAbsence personAbsence)
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

			if (!canRemovePersonAbsence(person, startDate))
			{
				return new[] {Resources.CouldNotRemoveAbsenceFromProtectedSchedule};
			}

			var scheduleDay = scheduleRange.ScheduledDay(startDate) as ExtractedSchedule;

			if (scheduleDay != null)
			{
				scheduleDay.Remove(personAbsence);
			}

			return _saveSchedulePartService.Save(scheduleDay, rules, KeepOriginalScheduleTag.Instance);
		}

		private IEnumerable<string> removePartPersonAbsenceFromScheduleDay(IPersonAbsence personAbsence,
			DateTimePeriod periodToRemove,
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

			if (!canRemovePersonAbsence(person, startDate))
			{
				return new[] {Resources.CouldNotRemoveAbsenceFromProtectedSchedule};
			}

			if (scheduleDay != null)
			{
				scheduleDay.Remove(personAbsence);
			}

			var errorMessages = _saveSchedulePartService.Save(scheduleDay, rules, KeepOriginalScheduleTag.Instance);
			if (errorMessages != null && errorMessages.Any())
			{
				return errorMessages;
			}

			errorMessages = new List<string>();
			var newAbsencePeriods = getPeriodsForNewAbsence(personAbsence.Period, periodToRemove);
			if (!newAbsencePeriods.Any()) return errorMessages;

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
					TrackedCommandInfo = trackedCommandInfo
				}, false);

				if (errors == null || !errors.Any()) continue;

				errorMessages = errorMessages.Concat(errors).ToList();
			}

			return errorMessages;
		}
	}
}
