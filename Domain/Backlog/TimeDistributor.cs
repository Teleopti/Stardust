using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Backlog
{
	public class TimeDistributor
	{
		public void Distribute(DateOnly date, TimeSpan timeToDistribute, List<IncomingTask> affectedTasks, PlannedTimeTypeEnum timeType)
		{
			foreach (var affectedTask in affectedTasks)
			{
				affectedTask.ClearTimeOnDate(date);
				//var unBookedTimeOnTask = affectedTask.GetUnBookedTime();
				affectedTask.SetTimeOnDate(date, TimeSpan.FromMinutes(1), timeType);
			}
		}
	}
}