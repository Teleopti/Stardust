﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Backlog
{
	public class IncomingTask : IBacklogTask
	{
		private readonly DateOnlyPeriod _spanningPeriod;
		private readonly int _totalWorkItems;
		private readonly TimeSpan _averageWorkTimePerItem;
		private readonly FlatDistributionSetter _distributionSetter;
		private readonly IDictionary<DateOnly, TaskDay> _taskDays = new Dictionary<DateOnly, TaskDay>();
		private IDictionary<DateOnly, TimeSpan> _orgPlannedTime = new Dictionary<DateOnly, TimeSpan>();
		private IDictionary<DateOnly, TimeSpan> _orgScheduledTime = new Dictionary<DateOnly, TimeSpan>();
		private IDictionary<DateOnly, bool> _manualPlannedTime = new Dictionary<DateOnly, bool>();
		private IDictionary<DateOnly, TimeSpan> _actualBacklog = new Dictionary<DateOnly, TimeSpan>();
		private double _incomingOverflowedWork;


		public IncomingTask(DateOnlyPeriod spanningPeriod, int totalWorkItems, TimeSpan averageWorkTimePerItem,
			FlatDistributionSetter distributionSetter)
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
			get { return new TimeSpan((long) TotalWorkItems*AverageWorkTimePerItem.Ticks); }
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

		public TimeSpan GetPlannedTimeOnDate(DateOnly date)
		{
			if (PlannedTimeTypeOnDate(date) != PlannedTimeTypeEnum.Scheduled)
				return GetTimeOnDate(date).Subtract(GetOverstaffTimeOnDate(date));

			return TimeSpan.Zero;
		}

		public TimeSpan GetRealPlannedTimeOnDate(DateOnly date)
		{
			return _orgPlannedTime[date];
		}

		public void SetRealPlannedTimeOnDate(DateOnly date, TimeSpan time)
		{
			if (_orgPlannedTime.ContainsKey(date))
			{
				_orgPlannedTime[date] = time;
			}
			else
			{
				_orgPlannedTime.Add(date, time);
			}
		}

		public void SetRealScheduledTimeOnDate(DateOnly date, TimeSpan time)
		{
			if (_orgScheduledTime.ContainsKey(date))
			{
				_orgScheduledTime[date] = time;
			}
			else
			{
				_orgScheduledTime.Add(date, time);
			}
		}

		public bool GetManualPlannedInfoOnDate(DateOnly date)
		{
			return _manualPlannedTime[date];
		}

		public void SetManualPlannedInfoOnDate(DateOnly date, bool isManualPlanned)
		{
			if (_manualPlannedTime.ContainsKey(date))
			{
				_manualPlannedTime[date] = isManualPlanned;
			}
			else
			{
				_manualPlannedTime.Add(date, isManualPlanned);
			}
		}

		public void SetActualBacklogOnDate(DateOnly date, TimeSpan backlog)
		{
			if (_actualBacklog.ContainsKey(date))
			{
				_actualBacklog[date] = backlog;
			}
			else
			{
				_actualBacklog.Add(date, backlog);
			}
		}

		public TimeSpan? GetActualBacklogOnDate(DateOnly date)
		{
			if (_actualBacklog.ContainsKey(date))
			{
				return _actualBacklog[date];
			}
			return null;
		}

		public TimeSpan GetRealScheduledTimeOnDate(DateOnly date)
		{
			return _orgScheduledTime[date];
		}

		public TimeSpan GetScheduledTimeOnDate(DateOnly date)
		{
			if (PlannedTimeTypeOnDate(date) == PlannedTimeTypeEnum.Scheduled)
				return GetTimeOnDate(date).Subtract(GetOverstaffTimeOnDate(date));

			return TimeSpan.Zero;
		}

		public PlannedTimeTypeEnum PlannedTimeTypeOnDate(DateOnly date)
		{
			return _taskDays[date].PlannedTimeType;
		}

		public void RecalculateDistribution()
		{
			_distributionSetter.Distribute(_taskDays.Values.ToList(), TotalWorkTime);
		}

		public void Close(DateOnly date)
		{
			_taskDays[date].Close();
		}

		public void SetTimeOnDate(DateOnly date, TimeSpan timeSpan, PlannedTimeTypeEnum timeType)
		{
			_taskDays[date].SetTime(timeSpan, timeType);
		}

		public TimeSpan GetBacklogOnDate(DateOnly date)
		{
			var planned = TimeSpan.Zero;
			var totalWorkTime = TotalWorkTime;

			foreach (var dateOnly in _taskDays.Keys)
			{
				if (dateOnly > date)
					break;
				
				planned = planned.Add(GetTimeOnDate(dateOnly));

				if (_actualBacklog.ContainsKey(dateOnly))
				{
					planned = TimeSpan.Zero;
					totalWorkTime = _actualBacklog[dateOnly];
				}
			}

			if (planned < totalWorkTime)
				return totalWorkTime.Subtract(planned);

			return TimeSpan.Zero;
		}

		public TimeSpan GetEstimatedOutgoingBacklogOnDate(DateOnly date)
		{
			if (date == SpanningPeriod.EndDate)
				return TimeSpan.Zero;

			var planned = TimeSpan.Zero;
			foreach (var dateOnly in _taskDays.Keys)
			{
				if (dateOnly <= date)
					planned = planned.Add(GetTimeOnDate(dateOnly));
			}

			if (planned < TotalWorkTime)
				return TotalWorkTime.Subtract(planned);

			return TimeSpan.Zero;
		}

		public TimeSpan GetEstimatedIncomingBacklogOnDate(DateOnly date)
		{
			var planned = TimeSpan.Zero;
			var totalWorkTime = TotalWorkTime;
			foreach (var dateOnly in _taskDays.Keys)
			{
				if (dateOnly >= date)
					break;
				
				planned = planned.Add(GetTimeOnDate(dateOnly));
				if (_actualBacklog.ContainsKey(dateOnly))
				{
					planned = TimeSpan.Zero;
					totalWorkTime = _actualBacklog[dateOnly];
				}
			}

			if (planned < totalWorkTime)
				return totalWorkTime.Subtract(planned);

			return TimeSpan.Zero;
		}

		public void ClearTimeOnDate(DateOnly date)
		{
			_taskDays[date].ClearTime();
		}

		public void Open(DateOnly date)
		{
			_taskDays[date].Open();
		}

		public TimeSpan GetOverstaffTimeOnDate(DateOnly date)
		{
			var backlog = GetEstimatedIncomingBacklogOnDate(date);
			var timeOnDate = GetTimeOnDate(date);

			if (timeOnDate > backlog)
				return timeOnDate.Subtract(backlog);

			return TimeSpan.Zero;
		}

		public TimeSpan GetTimeOutsideSLA()
		{
			var planned = TimeSpan.Zero;
			var totalWorkTime = TotalWorkTime;

			foreach (var dateOnly in _taskDays.Keys)
			{			
				planned = planned.Add(GetTimeOnDate(dateOnly));

				if (_actualBacklog.ContainsKey(dateOnly))
				{
					planned = TimeSpan.Zero;
					totalWorkTime = _actualBacklog[dateOnly];
				}
			}

			if (planned < totalWorkTime)
				return totalWorkTime.Subtract(planned);

			return TimeSpan.Zero;
		}
	}
}