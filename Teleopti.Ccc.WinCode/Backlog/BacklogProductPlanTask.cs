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

		public TimeSpan ForecastedTimeOnDate(DateOnly date, DateOnly productPlanStart)
		{
			if (_parent.SpanningDateOnlyPeriod().Contains(date) && !_parent.ClosedDays.Contains(date))
			{
				if (!_manualEntries.ContainsKey(date))
					return ForecastedTimePerDay(productPlanStart);

				return _manualEntries[date];
			}

			return TimeSpan.Zero;
		}

		public double ForecastedWorkOnDate(DateOnly date, DateOnly productPlanStart)
		{
			return ForecastedTimeOnDate(date, productPlanStart).Ticks/(double) _parent.IncomingAht.Ticks;
		}

		public TimeSpan ForecastedTimePerDay(DateOnly productPlanStart)
		{
			var numberOfEntries = 0;
			var totalTime = TimeSpan.Zero;
			foreach (var entry in _manualEntries.Where(e => e.Key <= productPlanStart))
			{
				numberOfEntries++;
				totalTime = totalTime.Add(entry.Value);
			}
			foreach (var dateOnly in _parent.SpanningDateOnlyPeriod().DayCollection())
			{
				if (dateOnly < productPlanStart)
					totalTime = totalTime.Add(_parent.BacklogScheduledTask.ScheduledTimeOnDate(dateOnly));
			}
			var timeToDistribute = TotalIncomingTime().Subtract(totalTime);
			var daysToDistributeOn = _parent.SpanningDateOnlyPeriod().DayCount() - _parent.ClosedDays.Count() - numberOfEntries;
			if(daysToDistributeOn > 0)
				return new TimeSpan(timeToDistribute.Ticks / daysToDistributeOn);

			return TimeSpan.Zero;
		}

		public TimeSpan PlannedBacklogTimeOnDate(DateOnly date, DateOnly productPlanStart)
		{
			if (!_parent.SpanningDateOnlyPeriod().Contains(date))
				return TimeSpan.Zero;

			var backlog = TotalIncomingTime();
			foreach (var dateOnly in _parent.SpanningDateOnlyPeriod().DayCollection())
			{
				if (dateOnly > date)
					break;
				if (!_parent.ClosedDays.Contains(dateOnly))
				{
					if(productPlanStart <= dateOnly)
					{
						backlog = backlog.Subtract(ForecastedTimeOnDate(dateOnly, productPlanStart));
					}
					else
					{
						backlog = backlog.Subtract(_parent.BacklogScheduledTask.ScheduledTimeOnDate(dateOnly));
					}
				}
			}

			return backlog;
		}

		public double PlannedBacklogWorkOnDate(DateOnly date, DateOnly productPlanStart)
		{
			return PlannedBacklogTimeOnDate(date, productPlanStart).Ticks/(double) _parent.IncomingAht.Ticks;
		}

		public TimeSpan PlannedTimeOnTask(DateOnly productPlanStart)
		{
			TimeSpan planned = TimeSpan.Zero;
			foreach (var dateOnly in _parent.SpanningDateOnlyPeriod().DayCollection())
			{
				planned = planned.Add(ForecastedTimeOnDate(dateOnly, productPlanStart));
			}

			return planned;
		}

		public double PlannedWorkOnTask(DateOnly productPlanStart)
		{
			return PlannedTimeOnTask(productPlanStart).Ticks / (double)_parent.IncomingAht.Ticks;
		}

		public TimeSpan PlannedBacklogTimeOnTask(DateOnly productPlanStart)
		{
			return TotalIncomingTime().Subtract(PlannedTimeOnTask(productPlanStart));
		}

		public double PlannedBacklogWorkOnTask(DateOnly productPlanStart)
		{
			return PlannedBacklogTimeOnTask(productPlanStart).Ticks / (double)_parent.IncomingAht.Ticks;
		}


		
	}
}