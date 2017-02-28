using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class UtcTimeZone : IUserTimeZone
	{
		public TimeZoneInfo TimeZone()
		{
			return TimeZoneInfo.Utc;
		}
	}
}