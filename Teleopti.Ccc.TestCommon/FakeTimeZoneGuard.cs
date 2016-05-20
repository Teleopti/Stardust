using System;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeTimeZoneGuard : ITimeZoneGuard
	{
		private TimeZoneInfo _timeZoneInfo;

		public FakeTimeZoneGuard(TimeZoneInfo timeZoneInfo)
		{
			_timeZoneInfo = timeZoneInfo;
		}

		public FakeTimeZoneGuard()
		{
			_timeZoneInfo = TimeZoneInfo.Utc;
		}

		public TimeZoneInfo CurrentTimeZone()
		{
			return _timeZoneInfo;
		}

		public void SetTimeZone(TimeZoneInfo timeZoneInfo)
		{
			_timeZoneInfo = timeZoneInfo;
		}
	}
}