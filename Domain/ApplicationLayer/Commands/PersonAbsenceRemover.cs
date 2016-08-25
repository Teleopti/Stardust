using System.Collections.Generic;
using System.Linq;
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
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly ISaveSchedulePartService _saveSchedulePartService;
		private readonly IPersonAbsenceCreator _personAbsenceCreator;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IAbsenceRequestCancelService _absenceRequestCancelService;
		private readonly ICheckingPersonalAccountDaysProvider _checkingPersonalAccountDaysProvider;

		public PersonAbsenceRemover(IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate,
			ISaveSchedulePartService saveSchedulePartService,
			IPersonAbsenceCreator personAbsenceCreator,
			ILoggedOnUser loggedOnUser,
			IAbsenceRequestCancelService absenceRequestCancelService, ICheckingPersonalAccountDaysProvider checkingPersonalAccountDaysProvider)
		{
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_saveSchedulePartService = saveSchedulePartService;
			_personAbsenceCreator = personAbsenceCreator;
			_loggedOnUser = loggedOnUser;
			_absenceRequestCancelService = absenceRequestCancelService;
			_checkingPersonalAccountDaysProvider = checkingPersonalAccountDaysProvider;
		}

		public IEnumerable<string> RemovePersonAbsence(DateOnly scheduleDate, IPerson person,
			IEnumerable<IPersonAbsence> personAbsences, IScheduleRange scheduleRange, TrackedCommandInfo commandInfo = null)
		{
			var errors = removePersonAbsenceFromScheduleDay(scheduleDate, person, personAbsences.ToList(), commandInfo,
				scheduleRange);
			return errors ?? new List<string>();
		}

		public IEnumerable<string> RemovePartPersonAbsence(DateOnly scheduleDate, IPerson person,
			IEnumerable<IPersonAbsence> personAbsences, DateTimePeriod periodToRemove, IScheduleRange scheduleRange,
			TrackedCommandInfo commandInfo = null)
		{
			var errors = removePersonAbsenceFromScheduleDay(scheduleDate, person, personAbsences.ToList(), commandInfo,
				scheduleRange, periodToRemove);
			return errors ?? new List<string>();
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

		private IEnumerable<string> removePersonAbsenceFromScheduleDay(
			DateOnly scheduleDate, IPerson person, IList<IPersonAbsence> personAbsences,
			TrackedCommandInfo commandInfo, IScheduleRange scheduleRange,
			DateTimePeriod? periodToRemove = null)
		{
			if (!canRemovePersonAbsence(person, scheduleDate))
			{
				return new[] {Resources.CouldNotRemoveAbsenceFromProtectedSchedule};
			}

			foreach (var personAbsence in personAbsences)
			{
				personAbsence.RemovePersonAbsence(commandInfo);
			}

			var scheduleDay = scheduleRange.ScheduledDay(scheduleDate) as ExtractedSchedule;
			if (scheduleDay != null)
			{
				var scheduleDaysForChecking = new List<IScheduleDay> {scheduleDay};
				scheduleDaysForChecking.AddRange(getScheduleDaysForCheckingAccount(personAbsences, scheduleDate, scheduleRange, person));

				foreach (var personAbsence in personAbsences)
				{
					scheduleDaysForChecking.ForEach(s => s.Remove(personAbsence));
				}

				var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);
				var errors = _saveSchedulePartService.Save(scheduleDaysForChecking, rules, KeepOriginalScheduleTag.Instance);

				if (errors != null && errors.Any())
				{
					return errors;
				}
			}

			var errorMessages = new List<string>();
			foreach (var personAbsence in personAbsences)
			{
				if (!periodToRemove.HasValue)
				{
					// Cancel the request if removing entire absence
					_absenceRequestCancelService.CancelAbsenceRequestsFromPersonAbsence(personAbsence);
					continue;
				}

				var newAbsencePeriods = getPeriodsForNewAbsence(personAbsence.Period, periodToRemove.Value);
				if (!newAbsencePeriods.Any())
				{
					// Cancel the request if removing part absence but no new absence need be created
					_absenceRequestCancelService.CancelAbsenceRequestsFromPersonAbsence(personAbsence);
					continue;
				}
				errorMessages.AddRange(createNewAbsencesForSplitAbsence(person, personAbsence,
					newAbsencePeriods, commandInfo, scheduleDay, scheduleRange).ToList());
			}

			return errorMessages;
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
					PersonRequest = personAbsence.PersonRequest,
					TrackedCommandInfo = commandInfo
				}, false);

				if (errors == null || !errors.Any()) continue;

				errorMessages.AddRange(errors);
			}

			return errorMessages;
		}

		/// <remarks>To be more efficient, we only return the first schedule day for each personal account for <see cref="Teleopti.Ccc.Domain.Scheduling.Rules.NewPersonAccountRule"/></remarks>
		private IEnumerable<IScheduleDay> getScheduleDaysForCheckingAccount(IEnumerable<IPersonAbsence> personAbsences,
			DateOnly startDate,
			IScheduleRange scheduleRange, IPerson person)
		{
			var scheduleDaysForChecking = new List<IScheduleDay>();
			var days = new HashSet<DateOnly>();

			foreach (var personAbsence in personAbsences)
			{
				var daysForChecking =
					_checkingPersonalAccountDaysProvider.GetDays(personAbsence.Layer.Payload, person,
						personAbsence.Period);
				foreach (var dayForChecking in daysForChecking)
				{
					if (dayForChecking != startDate && days.Add(dayForChecking))
					{
						scheduleDaysForChecking.Add(scheduleRange.ScheduledDay(dayForChecking));
					}
				}
			}

			return scheduleDaysForChecking;
		}
	}
}