using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class RemoveNonPreferenceDaysOffs
	{
		public void Execute(IScheduleDictionary schedules, 
			IEnumerable<IPerson> allAgents, 
			DateOnlyPeriod fullPeriod,
			IDictionary<IPerson, IEnumerable<DateOnly>> agentsWithExistingShiftsBeforeSchedule)
		{
			foreach (var agent in allAgents)
			{
				var range = schedules[agent];
				foreach (var date in fullPeriod.DayCollection())
				{
					if(agentsWithExistingShiftsBeforeSchedule.TryGetValue(agent, out var alreadyScheduledDates) && alreadyScheduledDates.Contains(date))
						continue;
					
					var scheduleDay = range.ScheduledDay(date);
					if (scheduleDay.HasDayOff() && (scheduleDay.PreferenceDay()?.Restriction.DayOffTemplate == null))
					{
						scheduleDay.DeleteDayOff();
					}

					schedules.Modify(scheduleDay, new DoNothingScheduleDayChangeCallBack());
				}
			}
		}
	}
}