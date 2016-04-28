using System;
using Teleopti.Ccc.Domain.Collection;
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
		 * * Change scheduled layers to be overtime layers
		 * * Change agent's current shift bag
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
			var personPeriod = agent.Period(date);
			var oldShiftBag = personPeriod.RuleSetBag;
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
			personPeriod.RuleSetBag = overtimePreferences.ShiftBagOvertimeScheduling;
			return new GenericDisposable(() =>
			{
				var currScheduleDay = scheduleDictionary[agent].ScheduledDay(date);
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
				scheduleDay.Person.Period(date).PersonContract.Contract.WorkTimeDirective = oldWorkTimeDirective;
				personPeriod.RuleSetBag = oldShiftBag;
			});
		}
	}
}