using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ReplaceActivityService
	{
		public void Replace(IEnumerable<IScheduleDay> scheduleDays, Activity activity, Activity replaceWithActivity, DateTimePeriod period)
		{
			foreach (var scheduleDay in scheduleDays)
			{
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
