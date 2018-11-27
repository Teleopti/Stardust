using System;
using System.Threading;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public static class TimeExtensions
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ITime));

		public static IDisposable StartTimerWithLock(this ITime time, Action callback, object @lock, TimeSpan period)
		{
			return time.StartTimer(state =>
			{
				try
				{
					if (!Monitor.TryEnter(@lock))
						return;
					try
					{
						callback.Invoke();
					}
					finally
					{
						Monitor.Exit(@lock);
					}
				}
				catch (Exception e)
				{
					logger.Error("Exception on timer tick", e);
				}
			}, null, period, period);
		}
	}

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