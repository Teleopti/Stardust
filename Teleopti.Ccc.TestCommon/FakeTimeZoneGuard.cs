using System;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeTimeZoneGuard : ITimeZoneGuard
	{
		private TimeZoneInfo _timeZone;
		
		public FakeTimeZoneGuard(TimeZoneInfo timeZoneInfo)
		{
			_timeZone = timeZoneInfo;
		}

		public FakeTimeZoneGuard()
		{
			_timeZone = TimeZoneInfo.Utc;
		}

		public TimeZoneInfo CurrentTimeZone()
		{
			return _timeZone;
		}

		public void Set(TimeZoneInfo timeZone)
		{
			_timeZone = timeZone;
		}
	}
}