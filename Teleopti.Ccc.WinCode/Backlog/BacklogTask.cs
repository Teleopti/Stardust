using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Backlog
{
	public class BacklogTask
	{
		private TimeSpan _incomingDemand;
		private TimeSpan _transferedBacklog;
		private readonly DateOnly _startDate;
		private TimeSpan _serviceLevel;
		private readonly IList<DateOnly> _closedDays = new List<DateOnly>();
		private IDictionary<DateOnly, TimeSpan> _manualEntries = new Dictionary<DateOnly, TimeSpan>();

		public BacklogTask(TimeSpan incomingDemand, DateOnly startDate, TimeSpan serviceLevel)
		{
			_incomingDemand = incomingDemand;
			_startDate = startDate;
			_serviceLevel = serviceLevel;
		}

		public DateOnly StartDate
		{
			get { return _startDate; }
		}

		public TimeSpan ServiceLevel
		{
			get { return _serviceLevel; }
		}

		public TimeSpan TransferedBacklog
		{
			get { return _transferedBacklog; }
			set { _transferedBacklog = value; }
		}

		public IEnumerable<DateOnly> ClosedDays
		{
			get { return _closedDays; }
		}

		public TimeSpan TotalIncoming()
		{
			return _incomingDemand.Add(TransferedBacklog);
		}

		public void SetManualEntry(DateOnly date, TimeSpan time)
		{
			if (!SpanningDateOnlyPeriod().Contains(date))
				return;

			if(!_manualEntries.ContainsKey(date))
				_manualEntries.Add(date,TimeSpan.Zero);

			_manualEntries[date] = time;
		}

		public void ClearManualEntry(DateOnly date)
		{
			_manualEntries.Remove(date);
		}

		public DateOnlyPeriod SpanningDateOnlyPeriod()
		{
			var endDate = new DateOnly(StartDate.Date.AddTicks(_serviceLevel.Ticks).Date).AddDays(-1);
			return new DateOnlyPeriod(_startDate, endDate);
		}

		public TimeSpan ForecastedTimeOnDate(DateOnly date)
		{
			if (SpanningDateOnlyPeriod().Contains(date) && !_closedDays.Contains(date))
			{
				if(!_manualEntries.ContainsKey(date))
					return ForecastedTimePerDay();

				return _manualEntries[date];
			}
				

			return TimeSpan.Zero;
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
			var timeToDistribute = TotalIncoming().Subtract(totalTime);
			var daysToDistributeOn = SpanningDateOnlyPeriod().DayCount() - _closedDays.Count - numberOfEntries;
			return new TimeSpan(timeToDistribute.Ticks / daysToDistributeOn);
		}

		public TimeSpan ForecastedBacklogOnDate(DateOnly date)
		{
			if (!SpanningDateOnlyPeriod().Contains(date))
				return TimeSpan.Zero;

			var backlog = TotalIncoming();
			foreach (var dateOnly in SpanningDateOnlyPeriod().DayCollection())
			{
				if(dateOnly > date)
					break;
				if(!_closedDays.Contains(dateOnly))
					backlog = backlog.Subtract(ForecastedTimeOnDate(dateOnly));
			}

			return backlog;
		}

		public void CloseDate(DateOnly date)
		{
			if (!SpanningDateOnlyPeriod().Contains(date))
				return;

			if(!_closedDays.Contains(date))
				_closedDays.Add(date);
		}

		//public TimeSpan TimePerDay(int preBookedDays, TimeSpan preBookedTime)
		//{
		//	return new TimeSpan(TotalIncoming().Add(-preBookedTime).Ticks / (SpanningDateOnlyPeriod().DayCount()-preBookedDays));
		//}
	}
}