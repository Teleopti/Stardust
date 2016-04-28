using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public static class OvertimeSchedulingScope
	{
		/*
		 * Temporarly (?) hack to fix...
		 * * We need to remove dayoffs otherwise no scheduling will occur
		 * * Need to "turn off" nightly rest rule if user has checked AllowBreakNightlyRest
		 * * Change scheduled layers to be overtime layers
		 */
		public static IDisposable Set(IScheduleDictionary scheduleDictionary, 
			IScheduleDay scheduleDay, 
			IOvertimePreferences overtimePreferences, 
			IScheduleTagSetter scheduleTagSetter, 
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			var orgPersonAss = scheduleDay.PersonAssignment(true).Clone() as IPersonAssignment;
			var hasDayOff = false;
			var oldWorkTimeDirective =scheduleDay.Person.Period(scheduleDay.DateOnlyAsPeriod.DateOnly).PersonContract.Contract.WorkTimeDirective;
			if (scheduleDay.HasDayOff())
			{
				scheduleDay.DeleteDayOff();
				scheduleDictionary.Modify(scheduleDay);
				hasDayOff = true;
			}
			if (overtimePreferences.AllowBreakNightlyRest)
			{
				scheduleDay.Person.Period(scheduleDay.DateOnlyAsPeriod.DateOnly).PersonContract.Contract.WorkTimeDirective = 
					new WorkTimeDirective(oldWorkTimeDirective.MinTimePerWeek, oldWorkTimeDirective.MaxTimePerWeek, TimeSpan.Zero, oldWorkTimeDirective.WeeklyRest);
			}
			return new GenericDisposable(() =>
			{
				var currScheduleDay = scheduleDictionary[scheduleDay.Person].ScheduledDay(scheduleDay.DateOnlyAsPeriod.DateOnly);
				var currLayers = currScheduleDay.PersonAssignment(true).MainActivities();
				currScheduleDay.Clear<IPersonAssignment>();

				if (hasDayOff)
				{
					orgPersonAss.SetThisAssignmentsDayOffOn(currScheduleDay.PersonAssignment(true));
				}
				currLayers.ForEach(x => currScheduleDay.CreateAndAddOvertime(x.Payload, x.Period, overtimePreferences.OvertimeType));

				var rules = NewBusinessRuleCollection.Minimum();
				if (!overtimePreferences.AllowBreakMaxWorkPerWeek)
				{
					rules.Add(new NewMaxWeekWorkTimeRule(new WeeksFromScheduleDaysExtractor()));
				}
				schedulePartModifyAndRollbackService.ModifyStrictly(currScheduleDay, scheduleTagSetter, rules);
				scheduleDay.Person.Period(scheduleDay.DateOnlyAsPeriod.DateOnly).PersonContract.Contract.WorkTimeDirective =
					oldWorkTimeDirective;
			});
		}
	}
}