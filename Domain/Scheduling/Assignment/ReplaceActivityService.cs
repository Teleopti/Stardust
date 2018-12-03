using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ReplaceActivityService
	{
		public void Replace(IEnumerable<IScheduleDay> scheduleDays, IActivity activity, IActivity replaceWithActivity, TimePeriod timePeriod, TimeZoneInfo timeZone)
		{
			foreach (var scheduleDay in scheduleDays)
			{
				var personAssignment = scheduleDay.PersonAssignment(true);
				foreach (var shiftLayer in personAssignment.ShiftLayers.OrderByDescending(x => x.OrderIndex))
				{
					var startDateTime = TimeZoneHelper.ConvertToUtc(shiftLayer.Period.StartDateTimeLocal(timeZone).Date.Add(timePeriod.StartTime), timeZone);
					var endDateTime = TimeZoneHelper.ConvertToUtc(shiftLayer.Period.StartDateTimeLocal(timeZone).Date.Add(timePeriod.EndTime), timeZone);
					var dateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
					if (!shiftLayer.Payload.Equals(activity) || !shiftLayer.Period.Contains(dateTimePeriod)) continue;
					personAssignment.InsertActivity(replaceWithActivity, dateTimePeriod, shiftLayer.OrderIndex + 1);
					break;
				}		
			}	
		}
	}
}
