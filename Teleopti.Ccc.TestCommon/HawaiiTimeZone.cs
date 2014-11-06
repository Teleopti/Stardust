using System;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class HawaiiTimeZone : IUserTimeZone
	{
		public TimeZoneInfo TimeZone()
		{
			return TimeZoneInfoFactory.HawaiiTimeZoneInfo();
		}
	}
}