﻿using System;
using System.Collections.Generic;
using System.Linq;
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
				
				var tempDayOneHasSwapData = CopyData(selectionOne[i], tempDayOne);
				var tempDayTwoHasSwapData = CopyData(selectionTwo[i], tempDayTwo);
			
				if (tempDayTwoHasSwapData)
				{
					selectionOne[i].Merge(tempDayTwo, false);
					schedulePartModifyAndRollbackService.Modify(selectionOne[i]);
				}
				else if (tempDayOneHasSwapData)
				{
					selectionOne[i].Merge(selectionOne[i], true);
					schedulePartModifyAndRollbackService.Modify(selectionOne[i]);	
				}


				if (tempDayOneHasSwapData)
				{
					selectionTwo[i].Merge(tempDayOne, false);
					schedulePartModifyAndRollbackService.Modify(selectionTwo[i]);
				}
				else if (tempDayTwoHasSwapData)
				{
					selectionTwo[i].Merge(selectionTwo[i], true);
					schedulePartModifyAndRollbackService.Modify(selectionTwo[i]);
				}
			}
		}

		private static bool CopyData(IScheduleDay scheduleDay, ISchedulePart tempDay)
		{
			var hasSwapData = false;

			if (scheduleDay == null)
				throw new ArgumentNullException("scheduleDay");

			if (tempDay == null)
				throw new ArgumentNullException("tempDay");

			if (scheduleDay.PersonAssignmentCollection().Count() > 0)
			{
				var personAssignment = scheduleDay.AssignmentHighZOrder();
				if (personAssignment.MainShift != null)
				{
					tempDay.AddMainShift(((IMainShift)personAssignment.MainShift.NoneEntityClone()));
					hasSwapData = true;
				}
			}

			if (scheduleDay.PersonDayOffCollection().Count > 0)
			{
				tempDay.Add(scheduleDay.PersonDayOffCollection()[0].NoneEntityClone());
				hasSwapData = true;
			}

			
			return hasSwapData;
		}	
	}
}
