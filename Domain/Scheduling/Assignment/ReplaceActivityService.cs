using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ReplaceActivityService
	{
		public void Replace(IEnumerable<IScheduleDay> scheduleDays, IActivity activity, IActivity replaceWithActivity, TimePeriod timePeriod)
		{
			foreach (var scheduleDay in scheduleDays)
			{
				var startDateTimeLocal = scheduleDay.Period.StartDateTimeLocal(TimeZoneGuard.Instance.CurrentTimeZone());
				var period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTimeLocal.Add(timePeriod.StartTime), 
					startDateTimeLocal.Add(timePeriod.EndTime), TimeZoneGuard.Instance.CurrentTimeZone());
				var personAssignment = scheduleDay.PersonAssignment(true);
				foreach (var shiftLayer in personAssignment.ShiftLayers.OrderByDescending(x => x.OrderIndex))
				{
					if (!shiftLayer.Payload.Equals(activity) || !shiftLayer.Period.Contains(period)) continue;
					personAssignment.InsertActivity(replaceWithActivity, period, shiftLayer.OrderIndex + 1);
					break;
				}		
			}	
		}
	}
}
