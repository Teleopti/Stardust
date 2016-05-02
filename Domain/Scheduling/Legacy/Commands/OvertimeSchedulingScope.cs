using System;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public static class OvertimeSchedulingScope
	{
		/*
		 * Temporarly hack to fix...
		 * * We need to remove dayoffs otherwise no scheduling will occur
		 * * Need to "turn off" nightly rest rule if user has checked AllowBreakNightlyRest
		 * ...we'll fix these one-by-one later
		 */
		public static IDisposable Set(IScheduleDictionary scheduleDictionary, 
			IScheduleDay scheduleDay, 
			IOvertimePreferences overtimePreferences, 
			IScheduleTagSetter scheduleTagSetter, 
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			var agent = scheduleDay.Person;
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var orgPersonAss = scheduleDay.PersonAssignment(true).EntityClone();
			var hasDayOff = false;
			var oldWorkTimeDirective = agent.Period(date).PersonContract.Contract.WorkTimeDirective;
			if (scheduleDay.HasDayOff())
			{
				scheduleDay.DeleteDayOff();
				scheduleDictionary.Modify(scheduleDay);
				hasDayOff = true;
			}
			if (overtimePreferences.AllowBreakNightlyRest)
			{
				agent.Period(date).PersonContract.Contract.WorkTimeDirective = 
					new WorkTimeDirective(oldWorkTimeDirective.MinTimePerWeek, oldWorkTimeDirective.MaxTimePerWeek, TimeSpan.Zero, oldWorkTimeDirective.WeeklyRest);
			}

			return new GenericDisposable(() =>
			{
				var currScheduleDay = scheduleDictionary[agent].ScheduledDay(date);
				if (hasDayOff)
				{
					orgPersonAss.SetThisAssignmentsDayOffOn(currScheduleDay.PersonAssignment(true));
				}
				schedulePartModifyAndRollbackService.ModifyStrictly(currScheduleDay, scheduleTagSetter, NewBusinessRuleCollection.Minimum());
				
				scheduleDay.Person.Period(date).PersonContract.Contract.WorkTimeDirective = oldWorkTimeDirective;
			});
		}
	}
}