using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITimeZoneGuard
	{
		TimeZoneInfo CurrentTimeZone();
		TimeZoneInfo TimeZone { get; }
		void Set(TimeZoneInfo timeZone);
	}
	
	public class TimeZoneGuard : ITimeZoneGuard
	{
		private TimeZoneInfo _timeZone;

		public TimeZoneInfo TimeZone => _timeZone ?? (_timeZone = TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone);

		public void Set(TimeZoneInfo timeZone)
		{
			_timeZone = timeZone;
		}

		public static ITimeZoneGuard Instance => ServiceLocatorForLegacy.TimeZoneGuard;

		public TimeZoneInfo CurrentTimeZone()
		{
			return TimeZone;
		}
	}
}