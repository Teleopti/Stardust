using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

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