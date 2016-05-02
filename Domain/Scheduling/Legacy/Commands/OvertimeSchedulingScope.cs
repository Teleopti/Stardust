using System;
using System.Linq;
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
		 * * AvailableAgentsOnly handled by "scheduling"
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
				var currLayers = currScheduleDay.PersonAssignment(true).MainActivities();
				currScheduleDay.Clear<IPersonAssignment>();
				if (hasDayOff)
				{
					orgPersonAss.SetThisAssignmentsDayOffOn(currScheduleDay.PersonAssignment(true));
				}
				currLayers.ForEach(x => currScheduleDay.CreateAndAddOvertime(x.Payload, x.Period, overtimePreferences.OvertimeType));
				var rollBackOnOvertimeAvailability = false;
				if (overtimePreferences.AvailableAgentsOnly)
				{
					var overtimeAvailability = currScheduleDay.PersistableScheduleDataCollection().OfType<IOvertimeAvailability>().FirstOrDefault();
					if (overtimeAvailability == null || !overtimeAvailability.Period.Contains(currScheduleDay.PersonAssignment(true).Period))
					{
						rollBackOnOvertimeAvailability = true;
					}
				}

				if (rollBackOnOvertimeAvailability)
				{
					schedulePartModifyAndRollbackService.Rollback();
				}
				else
				{
					schedulePartModifyAndRollbackService.ModifyStrictly(currScheduleDay, scheduleTagSetter, NewBusinessRuleCollection.Minimum());
				}
				
				scheduleDay.Person.Period(date).PersonContract.Contract.WorkTimeDirective = oldWorkTimeDirective;
			});
		}
	}
}