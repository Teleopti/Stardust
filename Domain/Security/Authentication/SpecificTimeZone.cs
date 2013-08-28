using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class SpecificTimeZone : IUserTimeZone
	{
		private readonly TimeZoneInfo _timeZone;

		public SpecificTimeZone(TimeZoneInfo timeZone)
		{
			_timeZone = timeZone;
		}

		public TimeZoneInfo TimeZone()
		{
			return _timeZone;
		}
	}
}