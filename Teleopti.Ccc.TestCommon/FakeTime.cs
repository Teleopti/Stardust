using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeTime : ITime
	{
		private readonly TestableNow _now;
		private readonly IList<FakeTimer> _timers = new List<FakeTimer>();

		public FakeTime(DateTime time) : this(new TestableNow(time))
		{
		}

		public FakeTime() : this(new TestableNow())
		{
		}

		public FakeTime(TestableNow now)
		{
			_now = now;
		}

		public DateTime UtcDateTime()
		{
			return _now.UtcDateTime();
		}

		private class FakeTimer
		{
			public TimerCallback Callback;
			public object State;
			public TimeSpan DueTime;
			public TimeSpan Period;
			public DateTime NextDueTime;
			public DateTime NextPeriodTime;
			public bool NextDueTimeHasPassed;
		}

		public object StartTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
		{
			var timer = new FakeTimer
			{
				Callback = callback,
				State = state,
				DueTime = dueTime,
				Period = period,
				NextDueTime = _now.UtcDateTime().Add(dueTime),
				NextPeriodTime = _now.UtcDateTime().Add(dueTime.Add(period)),
				NextDueTimeHasPassed = false,
			};
			_timers.Add(timer);
			return timer;
		}

		public void DisposeTimer(object timer)
		{
			_timers.Remove((FakeTimer) timer);
		}

		public void Passes(TimeSpan time)
		{
			var targetTime = _now.UtcDateTime().Add(time);
			var seconds = from s in Enumerable.Range(0, (int) targetTime.Subtract(_now.UtcDateTime()).TotalSeconds)
				select s;
			foreach (var s in seconds)
			{
				_now.Change(_now.UtcDateTime().AddSeconds(1));
				handleCallbackCall();
			}
		}

		private void handleCallbackCall()
		{
			foreach (var timer in _timers)
			{
				if (timer.Callback != null)
				{
					if (!timer.NextDueTimeHasPassed && _now.UtcDateTime() >= timer.NextDueTime)
					{
						timer.Callback(timer.State);
						timer.NextDueTimeHasPassed = true;
					}
					if (_now.UtcDateTime() >= timer.NextPeriodTime)
					{
						timer.Callback(timer.State);
						timer.NextPeriodTime = _now.UtcDateTime().Add(timer.Period);
					}
				}
			}
		}
	}
}