using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Backlog
{
	public class BacklogProductPlanTask
	{
		private readonly BacklogTask _parent;
		private TimeSpan _transferedBacklog;
		private IDictionary<DateOnly, TimeSpan> _manualEntries = new Dictionary<DateOnly, TimeSpan>();

		public BacklogProductPlanTask(BacklogTask parent)
		{
			_parent = parent;
		}

		public TimeSpan TransferedBacklog
		{
			get { return _transferedBacklog; }
			set { _transferedBacklog = value; }
		}

		public TimeSpan? OverStaff { get; set; }

		public TimeSpan TotalIncomingTime()
		{
			return _parent.IncomingTime.Add(TransferedBacklog);
		}

		public void SetManualEntry(DateOnly date, TimeSpan time)
		{
			if (!_parent.SpanningDateOnlyPeriod().Contains(date))
				return;

			if (!_manualEntries.ContainsKey(date))
				_manualEntries.Add(date, TimeSpan.Zero);

			_manualEntries[date] = time;
		}

		public void ClearManualEntry(DateOnly date)
		{
			_manualEntries.Remove(date);
		}

		public TimeSpan ForecastedTimeOnDate(DateOnly date)
		{
			if (_parent.SpanningDateOnlyPeriod().Contains(date) && !_parent.ClosedDays.Contains(date))
			{
				if (!_manualEntries.ContainsKey(date))
					return ForecastedTimePerDay();

				return _manualEntries[date];
			}

			return TimeSpan.Zero;
		}

		public double ForecastedWorkOnDate(DateOnly date)
		{
			return ForecastedTimeOnDate(date).Ticks/(double) _parent.IncomingAht.Ticks;
		}

		public TimeSpan ForecastedTimePerDay()
		{
			var numberOfEntries = 0;
			var totalTime = TimeSpan.Zero;
			foreach (var entry in _manualEntries.Values)
			{
				numberOfEntries++;
				totalTime = totalTime.Add(entry);
			}
			var timeToDistribute = TotalIncomingTime().Subtract(totalTime);
			var daysToDistributeOn = _parent.SpanningDateOnlyPeriod().DayCount() - _parent.ClosedDays.Count() - numberOfEntries;
			return new TimeSpan(timeToDistribute.Ticks / daysToDistributeOn);
		}

		public TimeSpan PlannedBacklogTimeOnDate(DateOnly date)
		{
			if (!_parent.SpanningDateOnlyPeriod().Contains(date))
				return TimeSpan.Zero;

			var backlog = TotalIncomingTime();
			foreach (var dateOnly in _parent.SpanningDateOnlyPeriod().DayCollection())
			{
				if (dateOnly > date)
					break;
				if (!_parent.ClosedDays.Contains(dateOnly))
					backlog = backlog.Subtract(ForecastedTimeOnDate(dateOnly));
			}

			return backlog;
		}

		public double PlannedBacklogWorkOnDate(DateOnly date)
		{
			return PlannedBacklogTimeOnDate(date).Ticks/(double) _parent.IncomingAht.Ticks;
		}

		public TimeSpan PlannedTimeOnTask()
		{
			TimeSpan planned = TimeSpan.Zero;
			foreach (var dateOnly in _parent.SpanningDateOnlyPeriod().DayCollection())
			{
				planned = planned.Add(ForecastedTimeOnDate(dateOnly));
			}

			return planned;
		}

		public double PlannedWorkOnTask()
		{
			return PlannedTimeOnTask().Ticks / (double)_parent.IncomingAht.Ticks;
		}

		public TimeSpan PlannedBacklogTimeOnTask()
		{
			return TotalIncomingTime().Subtract(PlannedTimeOnTask());
		}

		public double PlannedBacklogWorkOnTask()
		{
			return PlannedBacklogTimeOnTask().Ticks / (double)_parent.IncomingAht.Ticks;
		}


		
	}
}