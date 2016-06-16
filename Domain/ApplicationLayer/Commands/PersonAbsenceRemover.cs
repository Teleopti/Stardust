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

		public PersonAbsenceRemover(IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate,
			ISaveSchedulePartService saveSchedulePartService,
			IPersonAbsenceCreator personAbsenceCreator,
			ILoggedOnUser loggedOnUser,
			IAbsenceRequestCancelService absenceRequestCancelService)
		{
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_saveSchedulePartService = saveSchedulePartService;
			_personAbsenceCreator = personAbsenceCreator;
			_loggedOnUser = loggedOnUser;
			_absenceRequestCancelService = absenceRequestCancelService;
		}

		public IEnumerable<string> RemovePersonAbsence(DateOnly scheduleDate, IPerson person,
			IEnumerable<IPersonAbsence> personAbsences, IScheduleRange scheduleRange, TrackedCommandInfo commandInfo = null)
		{
			var errors =  removePersonAbsenceFromScheduleDay(scheduleDate, person,personAbsences.ToList(), commandInfo, scheduleRange);
			return errors ?? new List<string>();
		}

		public IEnumerable<string> RemovePartPersonAbsence(DateOnly scheduleDate, IPerson person,
			IEnumerable<IPersonAbsence> personAbsences, DateTimePeriod periodToRemove, IScheduleRange scheduleRange, TrackedCommandInfo commandInfo = null)
		{
			var errors = removePersonAbsenceFromScheduleDay(scheduleDate, person, personAbsences.ToList(), commandInfo, scheduleRange, periodToRemove);
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
		
		private IEnumerable<string> removePersonAbsenceFromScheduleDay (
			DateOnly scheduleDate, IPerson person, IList<IPersonAbsence> personAbsences,
			TrackedCommandInfo commandInfo, IScheduleRange scheduleRange,  DateTimePeriod? periodToRemove = null )
		{
			foreach (var personAbsence in personAbsences)
			{
				personAbsence.RemovePersonAbsence (commandInfo);
			}

			var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange (scheduleRange);
			var scheduleDay = scheduleRange.ScheduledDay (scheduleDate) as ExtractedSchedule;

			if (!canRemovePersonAbsence (person, scheduleDate))
			{
				return new[] {Resources.CouldNotRemoveAbsenceFromProtectedSchedule};
			}

			if (scheduleDay != null)
			{
				foreach (var personAbsence in personAbsences)
				{
					scheduleDay.Remove (personAbsence);
				}
			}

			var errorMessages = _saveSchedulePartService.Save (scheduleDay, rules, KeepOriginalScheduleTag.Instance);
			if (errorMessages != null && errorMessages.Any())
			{
				return errorMessages;
			}

			if (periodToRemove.HasValue)
			{
				errorMessages = createNewAbsencesForSplitAbsence(person, personAbsences, periodToRemove.Value, commandInfo, scheduleDay, scheduleRange);
			}

			// TODO: #39138,#39065 - have caused the cancellation functionality to be reverted
			//_absenceRequestCancelService.CancelAbsenceRequestsFromPersonAbsences(personAbsences);

			return errorMessages;
		}

		private IList<string> createNewAbsencesForSplitAbsence (IPerson person, IEnumerable<IPersonAbsence> personAbsences,
			DateTimePeriod periodToRemove, TrackedCommandInfo commandInfo, IScheduleDay scheduleDay,
			IScheduleRange scheduleRange)
		{
			IList<string> errorMessages = new List<string>();
			foreach (var personAbsence in personAbsences)
			{
				var newAbsencePeriods = getPeriodsForNewAbsence (personAbsence.Period, periodToRemove);
				if (!newAbsencePeriods.Any()) return errorMessages;

				foreach (var period in newAbsencePeriods)
				{
					// xinfli: The second parameter "isFullDayAbsence" doesn't matter, since it just raise different event
					// and all the events will be converted to "ScheduleChangedEvent" (Refer to ScheduleChangedEventPublisher class)
					var errors = _personAbsenceCreator.Create (new AbsenceCreatorInfo
					{
						Person = person,
						Absence = personAbsence.Layer.Payload,
						ScheduleDay = scheduleDay,
						ScheduleRange = scheduleRange,
						AbsenceTimePeriod = period,
						TrackedCommandInfo = commandInfo
					}, false);

					if (errors == null || !errors.Any()) continue;

					errorMessages = errorMessages.Concat (errors).ToList();
				}
			}

			return errorMessages;
		}
		
	}
}
