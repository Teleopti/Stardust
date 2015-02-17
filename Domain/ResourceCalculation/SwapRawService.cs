using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SwapRawService : ISwapRawService
	{
        private readonly IPrincipalAuthorization _authorizationService;

        public SwapRawService(IPrincipalAuthorization authorizationService)
        {
            _authorizationService = authorizationService;
        }

		public void Swap(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, IList<IScheduleDay> selectionOne, IList<IScheduleDay> selectionTwo, IDictionary<IPerson, IList<DateOnly>> locks)
		{
			if (selectionOne == null)
				throw new ArgumentNullException("selectionOne");

			if (selectionTwo == null)
				throw new ArgumentNullException("selectionTwo");

			if(locks == null)
				throw new ArgumentNullException("locks");

			if (schedulePartModifyAndRollbackService == null)
				throw new ArgumentNullException("schedulePartModifyAndRollbackService");

			if (selectionOne.Count != selectionTwo.Count)
				throw new ArgumentException("selections must be of same size");

			for (var i = 0; i < selectionOne.Count; i++)
			{
				var tempDayOne = ExtractedSchedule.CreateScheduleDay(selectionOne[i].Owner, selectionOne[i].Person, selectionOne[i].Period.ToDateOnlyPeriod(selectionOne[i].TimeZone).StartDate);
				var tempDayTwo = ExtractedSchedule.CreateScheduleDay(selectionTwo[i].Owner, selectionTwo[i].Person, selectionTwo[i].Period.ToDateOnlyPeriod(selectionTwo[i].TimeZone).StartDate);

                if (!_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, tempDayOne.DateOnlyAsPeriod.DateOnly, tempDayOne.Person) || !_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, tempDayTwo.DateOnlyAsPeriod.DateOnly, tempDayTwo.Person))
                    throw new PermissionException("No permission to change the person assignment");
                
                if(locks.ContainsKey(tempDayOne.Person))
				{
					var lockedDates = locks[tempDayOne.Person];
					if(lockedDates.Contains(tempDayOne.DateOnlyAsPeriod.DateOnly))
						continue;
				}

				if (locks.ContainsKey(tempDayTwo.Person))
				{
					var lockedDates = locks[tempDayTwo.Person];
					if (lockedDates.Contains(tempDayTwo.DateOnlyAsPeriod.DateOnly))
						continue;
				}
				
				var tempDayOneHasSwapData = copyData(selectionOne[i], tempDayOne);
				var tempDayTwoHasSwapData = copyData(selectionTwo[i], tempDayTwo);
			
				if (tempDayTwoHasSwapData)
				{
					var tmpAbsences = getAndRemoveAbsences(selectionOne[i]);
					selectionOne[i].Merge(tempDayTwo, false, true);
					modifyDay(tmpAbsences, selectionOne[i], schedulePartModifyAndRollbackService);
				}
				else if (tempDayOneHasSwapData)
				{
					var tmpAbsences = getAndRemoveAbsences(selectionOne[i]);
					selectionOne[i].DeleteOvertime();
					selectionOne[i].Merge(selectionOne[i], true, true);
					modifyDay(tmpAbsences, selectionOne[i], schedulePartModifyAndRollbackService);	
				}

				if (tempDayOneHasSwapData)
				{
					var tmpAbsences = getAndRemoveAbsences(selectionTwo[i]);
					selectionTwo[i].DeleteOvertime();
					selectionTwo[i].Merge(tempDayOne, false, true);
					modifyDay(tmpAbsences, selectionTwo[i], schedulePartModifyAndRollbackService);
				}
				else if (tempDayTwoHasSwapData)
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

		private static bool copyData(IScheduleDay scheduleDay, IScheduleDay tempDay)
		{
			var hasSwapData = false;

			InParameter.NotNull("scheduleDay", scheduleDay);
			InParameter.NotNull("tempDay", tempDay);

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
