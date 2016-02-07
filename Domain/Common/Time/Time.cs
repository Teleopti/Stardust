using System;
using System.Threading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public class Time : ITime
	{
		private readonly INow _now;

		public Time(INow now)
		{
			_now = now;
		}

		public DateTime UtcDateTime()
		{
			return _now.UtcDateTime();
		}

		public IDisposable StartTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
		{
			var timer = new Timer(callback, state, dueTime, period);
			return new GenericDisposable(() => timer.Dispose());
		}
	}
}