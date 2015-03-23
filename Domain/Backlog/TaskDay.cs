using System;

namespace Teleopti.Ccc.Domain.Backlog
{
	public class TaskDay
	{
		private TimeSpan _time;
		private PlannedTimeTypeEnum _plannedTimeType = PlannedTimeTypeEnum.Actual;

		public TimeSpan Time
		{
			get { return _time; }
		}

		public PlannedTimeTypeEnum PlannedTimeType
		{
			get { return _plannedTimeType; }
		}

		public void SetTime(TimeSpan time, PlannedTimeTypeEnum plannedTimeType)
		{
			_time = time;
			_plannedTimeType = plannedTimeType;
		}
	}

	public enum PlannedTimeTypeEnum
	{
		Actual,
 		Scheduled,
		Planned,
		Manual
	}
}