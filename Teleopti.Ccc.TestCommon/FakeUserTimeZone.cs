using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeUserTimeZone : IUserTimeZone
	{
		private readonly TimeZoneInfo _timeZone;

		public FakeUserTimeZone(TimeZoneInfo timeZone)
		{
			_timeZone = timeZone;
		}

		public TimeZoneInfo TimeZone()
		{
			return _timeZone;
		}
	}
}