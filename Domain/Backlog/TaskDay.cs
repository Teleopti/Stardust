using System;

namespace Teleopti.Ccc.Domain.Backlog
{
	public class TaskDay
	{
		private TimeSpan _time;
		private PlannedTimeTypeEnum _plannedTimeType = PlannedTimeTypeEnum.Calculated;
		private TimeSpan? _actualBacklog;
		public void SetTime(TimeSpan time, PlannedTimeTypeEnum plannedTimeType)
		{
			_time = time;
			_plannedTimeType = plannedTimeType;
		}

		public TimeSpan Time
		{
			get { return _time; }
		}

		public PlannedTimeTypeEnum PlannedTimeType
		{
			get { return _plannedTimeType; }
		}

		public void SetActualBacklog(int numberOfTasks, TimeSpan averageWorkTimePerTask)
		{
			_actualBacklog = new TimeSpan(numberOfTasks*averageWorkTimePerTask.Ticks);
		}

		public TimeSpan? ActualBacklog
		{
			get { return _actualBacklog; }
		}

		public void Close()
		{
			_plannedTimeType = PlannedTimeTypeEnum.Closed;
		}

		public void ClearTime()
		{
			throw new NotImplementedException();
		}
	}

	public enum PlannedTimeTypeEnum
	{
 		Scheduled,
		Calculated,
		Manual,
		Closed
	}
}