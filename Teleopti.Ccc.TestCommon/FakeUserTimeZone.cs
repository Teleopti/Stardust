using System;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeUserTimeZone : IUserTimeZone
	{
		private TimeZoneInfo _timeZone;

		public FakeUserTimeZone(TimeZoneInfo timeZone)
		{
			_timeZone = timeZone;
		}

		public TimeZoneInfo TimeZone()
		{
			return _timeZone;
		}

		private void Is(TimeZoneInfo timeZone)
		{
			_timeZone = timeZone;
		}

		public void IsHawaii()
		{
			Is(TimeZoneInfoFactory.HawaiiTimeZoneInfo());
		}
	}
}