﻿using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class DoNothingScheduleDayChangeCallBack : IScheduleDayChangeCallback
	{
		public void ScheduleDayChanging(IScheduleDay partBefore)
		{
			//do nothing
		}

		public void ScheduleDayChanged(IScheduleDay partAfter)
		{
			//do nothing
		}
	}
}