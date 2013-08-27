using System;
using Teleopti.Interfaces.Domain;

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