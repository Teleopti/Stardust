using System;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeTimeZoneGuard : ITimeZoneGuard
	{
		private readonly TimeZoneInfo _timeZoneInfo;

		public FakeTimeZoneGuard(TimeZoneInfo timeZoneInfo)
		{
			_timeZoneInfo = timeZoneInfo;
		}

		public TimeZoneInfo CurrentTimeZone()
		{
			return _timeZoneInfo;
		}
	}
}