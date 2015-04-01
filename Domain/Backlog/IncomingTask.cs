using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Backlog
{
	public class IncomingTask
	{
		private readonly DateOnlyPeriod _spanningPeriod;
		private readonly int _totalWorkItems;
		private readonly TimeSpan _averageWorkTimePerItem;
		private readonly FlatDistributionSetter _distributionSetter;
		private readonly IDictionary<DateOnly, TaskDay> _taskDays = new Dictionary<DateOnly, TaskDay>();
		private double _incomingOverflowedWork;


		public IncomingTask(DateOnlyPeriod spanningPeriod, int totalWorkItems, TimeSpan averageWorkTimePerItem, FlatDistributionSetter distributionSetter)
		{
			_spanningPeriod = spanningPeriod;
			_totalWorkItems = totalWorkItems;
			_averageWorkTimePerItem = averageWorkTimePerItem;
			_distributionSetter = distributionSetter;
			foreach (var dateOnly in spanningPeriod.DayCollection())
			{
				_taskDays.Add(dateOnly, new TaskDay());
			}
		}

		public DateOnlyPeriod SpanningPeriod
		{
			get { return _spanningPeriod; }
		}

		public double TotalWorkItems
		{
			get { return _totalWorkItems + _incomingOverflowedWork; }
		}

		public TimeSpan TotalWorkTime
		{
			get { return new TimeSpan((long)TotalWorkItems*AverageWorkTimePerItem.Ticks);}
		}

		public TimeSpan AverageWorkTimePerItem
		{
			get { return _averageWorkTimePerItem; }
		}

		public void SetIncomingOverflowedWork(double workItems)
		{
			_incomingOverflowedWork = workItems;
		}

		public TimeSpan GetTimeOnDate(DateOnly date)
		{
			return _taskDays[date].Time;
		}

		public PlannedTimeTypeEnum PlannedTimeTypeOnDate(DateOnly date)
		{
			return _taskDays[date].PlannedTimeType;
		}

		public void RecalculateDistribution()
		{
			_distributionSetter.Distribute(_taskDays.Values.ToList(), AverageWorkTimePerItem);
		}

		public void Close(DateOnly date)
		{
			_taskDays[date].Close();
			RecalculateDistribution();
		}

		public void SetTimeOnDate(DateOnly date, TimeSpan timeSpan, PlannedTimeTypeEnum timeType)
		{
			_taskDays[date].SetTime(timeSpan, timeType);
		}

		public TimeSpan GetUnBookedTime()
		{
			var bookedTime = TimeSpan.Zero;
			foreach (var taskDay in _taskDays.Values)
			{
				if (taskDay.PlannedTimeType == PlannedTimeTypeEnum.Manual ||
				    taskDay.PlannedTimeType == PlannedTimeTypeEnum.Scheduled)
					bookedTime = bookedTime.Add(taskDay.Time);
			}

			return TotalWorkTime.Subtract(bookedTime);
		}

		public void ClearTimeOnDate(DateOnly date)
		{
			_taskDays[date].ClearTime();
		}
	}
}