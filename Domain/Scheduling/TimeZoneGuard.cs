using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITimeZoneGuard
	{
		TimeZoneInfo CurrentTimeZone();
		void Set(TimeZoneInfo timeZone);
	}
	
	public class TimeZoneGuard : ITimeZoneGuard
	{
		private TimeZoneInfo _timeZone;

		public void Set(TimeZoneInfo timeZone)
		{
			_timeZone = timeZone;
		}

		public static ITimeZoneGuard Instance => ServiceLocator_DONTUSE.TimeZoneGuard;

		public TimeZoneInfo CurrentTimeZone()
		{
			return _timeZone ?? (_timeZone = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
		}
	}
}