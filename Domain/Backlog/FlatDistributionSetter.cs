using System;
using System.Collections;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Backlog
{
	public class FlatDistributionSetter
	{
		public void Distribute(IList<TaskDay> taskDays, TimeSpan totalWorkTime)
		{
			var daysToDistributeOn = taskDays.Count;
			var manualOrScheduledTime = TimeSpan.Zero;
			foreach (var taskDay in taskDays)
			{
				if (taskDay.PlannedTimeType == PlannedTimeTypeEnum.Scheduled ||
				    taskDay.PlannedTimeType == PlannedTimeTypeEnum.Manual)
				{
					manualOrScheduledTime = manualOrScheduledTime.Add(taskDay.Time);
					daysToDistributeOn--;
				}

				if (taskDay.PlannedTimeType == PlannedTimeTypeEnum.Closed)
					daysToDistributeOn--;
			}

			var timeToDistribute = totalWorkTime.Subtract(manualOrScheduledTime);
			var timeOnDay = TimeSpan.Zero;
			if(daysToDistributeOn > 0 && timeToDistribute > TimeSpan.Zero)
				 timeOnDay = new TimeSpan(timeToDistribute.Ticks/daysToDistributeOn);

			var remainingDays = daysToDistributeOn;
			foreach (var taskDay in taskDays)
			{
				if (taskDay.ActualBacklog.HasValue)
				{
					timeOnDay = TimeSpan.Zero;
					if (remainingDays > 0 && taskDay.ActualBacklog.Value > TimeSpan.Zero)
						timeOnDay = new TimeSpan(taskDay.ActualBacklog.Value.Ticks / remainingDays);
				}
				
				if(taskDay.PlannedTimeType == PlannedTimeTypeEnum.Calculated)
				{
					taskDay.SetTime(timeOnDay, PlannedTimeTypeEnum.Calculated);
					remainingDays--;
				}
			}
		}
	}
}