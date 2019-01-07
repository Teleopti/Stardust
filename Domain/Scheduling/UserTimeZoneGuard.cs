using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class UserTimeZoneGuard : ITimeZoneGuard
	{
		private readonly IUserTimeZone _userTimeZone;

		public UserTimeZoneGuard(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}

		public TimeZoneInfo TimeZone 
		{
			get => _userTimeZone.TimeZone();
			set { }
		}
		public TimeZoneInfo CurrentTimeZone()
		{
			return TimeZone;
		}
	}
}