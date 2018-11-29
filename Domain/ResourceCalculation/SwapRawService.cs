using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SwapRawService : ISwapRawService
	{
        private readonly ICurrentAuthorization _authorizationService;

        public SwapRawService(ICurrentAuthorization authorizationService)
        {
            _authorizationService = authorizationService;
        }

		public void Swap(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, IList<IScheduleDay> selectionOne, IList<IScheduleDay> selectionTwo, IDictionary<IPerson, IList<DateOnly>> locks)
		{
			InParameter.NotNull(nameof(selectionOne), selectionOne);
			InParameter.NotNull(nameof(selectionTwo), selectionTwo);
			InParameter.NotNull(nameof(locks), locks);
			InParameter.NotNull(nameof(schedulePartModifyAndRollbackService), schedulePartModifyAndRollbackService);
			InParameter.ListsHaveSameSize(selectionOne, selectionTwo);

			for (var i = 0; i < selectionOne.Count; i++)
			{
				var tempDayOne = ExtractedSchedule.CreateScheduleDay(selectionOne[i].Owner, selectionOne[i].Person, selectionOne[i].Period.ToDateOnlyPeriod(selectionOne[i].TimeZone).StartDate, _authorizationService);
				var tempDayTwo = ExtractedSchedule.CreateScheduleDay(selectionTwo[i].Owner, selectionTwo[i].Person, selectionTwo[i].Period.ToDateOnlyPeriod(selectionTwo[i].TimeZone).StartDate, _authorizationService);

                if (!_authorizationService.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, tempDayOne.DateOnlyAsPeriod.DateOnly, tempDayOne.Person) || !_authorizationService.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, tempDayTwo.DateOnlyAsPeriod.DateOnly, tempDayTwo.Person))
                    throw new PermissionException("No permission to change the person assignment");

				IList<DateOnly> lockedDates;
				if(locks.TryGetValue(tempDayOne.Person, out lockedDates))
				{
					if(lockedDates.Contains(tempDayOne.DateOnlyAsPeriod.DateOnly))
						continue;
				}

				if (locks.TryGetValue(tempDayTwo.Person, out lockedDates))
				{
					if (lockedDates.Contains(tempDayTwo.DateOnlyAsPeriod.DateOnly))
						continue;
				}
				
				var tempDayOneHasShiftOrDayOff = hasShiftOrDayOff(selectionOne[i], tempDayOne);
				var tempDayTwoHasShiftOrDayOff = hasShiftOrDayOff(selectionTwo[i], tempDayTwo);
			
				if (tempDayTwoHasShiftOrDayOff)
				{
					var tmpAbsences = getAndRemoveAbsences(selectionOne[i]);
					selectionOne[i].Merge(tempDayTwo, false, true);
					modifyDay(tmpAbsences, selectionOne[i], schedulePartModifyAndRollbackService);
				}
				else if (tempDayOneHasShiftOrDayOff)
				{
					var tmpAbsences = getAndRemoveAbsences(selectionOne[i]);
					selectionOne[i].DeleteOvertime();
					selectionOne[i].Merge(selectionOne[i], true, true);
					modifyDay(tmpAbsences, selectionOne[i], schedulePartModifyAndRollbackService);	
				}

				if (tempDayOneHasShiftOrDayOff)
				{
					var tmpAbsences = getAndRemoveAbsences(selectionTwo[i]);
					selectionTwo[i].DeleteOvertime();
					selectionTwo[i].Merge(tempDayOne, false, true);
					modifyDay(tmpAbsences, selectionTwo[i], schedulePartModifyAndRollbackService);
				}
				else if (tempDayTwoHasShiftOrDayOff)
				{
					var tmpAbsences = getAndRemoveAbsences(selectionTwo[i]);
					selectionTwo[i].Merge(selectionTwo[i], true, true);
					modifyDay(tmpAbsences, selectionTwo[i], schedulePartModifyAndRollbackService);
				}
			}
		}

		private IEnumerable<IPersonAbsence> getAndRemoveAbsences(IScheduleDay scheduleDay)
		{
			var tmpAbsences = scheduleDay.PersonAbsenceCollection();

			foreach (var personAbsence in tmpAbsences)
			{
				scheduleDay.Remove(personAbsence);
			}

			return tmpAbsences;
		}

		private void modifyDay(IEnumerable<IPersonAbsence> preservedAbsences, IScheduleDay scheduleDay, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			foreach (var personAbsence in preservedAbsences)
			{
				scheduleDay.Add(personAbsence);
			}

			schedulePartModifyAndRollbackService.Modify(scheduleDay);
		}

		private static bool hasShiftOrDayOff(IScheduleDay scheduleDay, IScheduleDay tempDay)
		{
			var hasSwapData = false;

			InParameter.NotNull(nameof(scheduleDay), scheduleDay);
			InParameter.NotNull(nameof(tempDay), tempDay);

			var personAssignment = scheduleDay.PersonAssignment();
			if (personAssignment != null)
			{
				if (personAssignment.ShiftCategory != null)
				{
					tempDay.AddMainShift(scheduleDay.PersonAssignment());
					hasSwapData = true;
				}
				if (personAssignment.DayOff() != null)
				{
					personAssignment.SetThisAssignmentsDayOffOn(tempDay.PersonAssignment(true));
					hasSwapData = true;
				}
			}
			
			return hasSwapData;
		}	
	}
}
