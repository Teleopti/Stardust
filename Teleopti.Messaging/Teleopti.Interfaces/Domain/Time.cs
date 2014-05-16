using System;
using System.Collections.Generic;
using System.Threading;

namespace Teleopti.Interfaces.Domain
{
	public class Time : ITime
	{
		private readonly INow _now;
		private readonly IList<Timer> _timers = new List<Timer>();

		public Time(INow now)
		{
			_now = now;
		}

		public DateTime UtcDateTime()
		{
			return _now.UtcDateTime();
		}

		public object StartTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
		{
			var timer = new Timer(callback, state, dueTime, period);
			_timers.Add(timer);
			return timer;
		}

		public void DisposeTimer(object timer)
		{
			var t = (Timer) timer;
			t.Dispose();
		}
	}
}