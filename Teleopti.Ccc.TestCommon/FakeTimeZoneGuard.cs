using System;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeTimeZoneGuard : ITimeZoneGuard
	{
		public FakeTimeZoneGuard(TimeZoneInfo timeZoneInfo)
		{
			TimeZone = timeZoneInfo;
		}

		public FakeTimeZoneGuard()
		{
			TimeZone = TimeZoneInfo.Utc;
		}

		public TimeZoneInfo CurrentTimeZone()
		{
			return TimeZone;
		}

		public TimeZoneInfo TimeZone { get; private set; }

		public void Set(TimeZoneInfo timeZone)
		{
			TimeZone = timeZone;
		}

		public void SetTimeZone(TimeZoneInfo timeZoneInfo)
		{
			TimeZone = timeZoneInfo;
		}
	}
}