using System;
using System.Linq;
using System.Threading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeTime : ITime
	{
		private TimerCallback _callback;
		private TimeSpan _period;
		private object _state;
		private DateTime _nextDueTime;
		private DateTime _nextPeriodTime;
		private readonly TestableNow _now;
		private bool _nextDueTimeHasPassed;

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

		public void StartTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
		{
			_callback = callback;
			_state = state;
			_period = period;
			_nextDueTime = _now.UtcDateTime().Add(dueTime);
			_nextPeriodTime = _now.UtcDateTime().Add(dueTime.Add(period));
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
			if (_callback != null)
			{
				if (!_nextDueTimeHasPassed && _now.UtcDateTime() >= _nextDueTime)
				{
					_callback(_state);
					_nextDueTimeHasPassed = true;
				}
				if (_now.UtcDateTime() >= _nextPeriodTime)
				{
					_callback(_state);
					_nextPeriodTime = _now.UtcDateTime().Add(_period);
				}
			}
		}
	}
}