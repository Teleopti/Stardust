using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class RemoveNonPreferenceDaysOffs
	{
		public void Execute(IScheduleDictionary schedules, IEnumerable<IPerson> agents, DateOnlyPeriod selectedPeriod)
		{
			foreach (var agent in agents)
			{
				var range = schedules[agent];
				foreach (var date in selectedPeriod.DayCollection())
				{
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