using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Backlog
{
	public class BacklogScheduledTask
	{
		private readonly BacklogTask _parent;
		private TimeSpan _transferedBacklog;
		private TimeSpan _overStaff;
		private IDictionary<DateOnly, TimeSpan> _scheduledTimes = new Dictionary<DateOnly, TimeSpan>();

		public BacklogScheduledTask(BacklogTask parent)
		{
			_parent = parent;
		}

		public TimeSpan TransferedBacklog
		{
			get { return _transferedBacklog; }
			set { _transferedBacklog = value; }
		}

		public TimeSpan OverStaff
		{
			get { return _overStaff; }
			set { _overStaff = value; }
		}

		public TimeSpan TotalIncoming()
		{
			return _parent.IncomingTime.Add(TransferedBacklog);
		}

		public TimeSpan ScheduledBacklogTimeOnTask()
		{
			return TotalIncoming().Subtract(ScheduledTimeOnTask());
		}

		public double ScheduledBacklogWorkOnTask()
		{
			return ScheduledBacklogTimeOnTask().Ticks / (double)_parent.IncomingAht.Ticks;
		}

		public Percent ScheduledServiceLevelOnTask()
		{
			var ticks = 0d;
			if (TotalIncoming().Ticks > 0)
				ticks = ScheduledTimeOnTask().Ticks / (double)TotalIncoming().Ticks;

			return new Percent(ticks);
		}

		public void SetScheduledTime(DateOnly date, TimeSpan time)
		{
			if (!_parent.SpanningDateOnlyPeriod().Contains(date))
				return;

			if (!_scheduledTimes.ContainsKey(date))
				_scheduledTimes.Add(date, TimeSpan.Zero);

			_scheduledTimes[date] = time;
		}

		public TimeSpan ScheduledTimeOnDate(DateOnly date)
		{
			if (_scheduledTimes.ContainsKey(date))
				return _scheduledTimes[date];

			return TimeSpan.Zero;
		}

		public TimeSpan ScheduledTimeOnTask()
		{
			return new TimeSpan(_scheduledTimes.Values.Sum(t => t.Ticks));
		}

		public double ScheduledWorkOnTask()
		{
			return ScheduledTimeOnTask().Ticks/(double) _parent.IncomingAht.Ticks;
		}

		public TimeSpan ScheduledBackLogTimeOnDate(DateOnly date)
		{
			if (!_parent.SpanningDateOnlyPeriod().Contains(date))
				return TimeSpan.Zero;

			var backlog = TotalIncoming();
			foreach (var dateOnly in _parent.SpanningDateOnlyPeriod().DayCollection())
			{
				if (dateOnly > date)
					break;

				if (_scheduledTimes.ContainsKey(dateOnly))
					backlog = backlog.Subtract(_scheduledTimes[dateOnly]);
			}

			return backlog;
		}

		public double ScheduledBacklogWorkOnDate(DateOnly date)
		{
			return ScheduledBackLogTimeOnDate(date).Ticks / (double)_parent.IncomingAht.Ticks;
		}


		
	}
}