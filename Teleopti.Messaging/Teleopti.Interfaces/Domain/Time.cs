using System;
using System.Threading;

namespace Teleopti.Interfaces.Domain
{
	public class Time : ITime
	{
		private readonly INow _now;
		private Timer _timer;

		public Time(INow now)
		{
			_now = now;
		}

		public DateTime UtcDateTime()
		{
			return _now.UtcDateTime();
		}

		public void StartTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
		{
			_timer = new Timer(callback, state, dueTime, period);
		}
	}
}