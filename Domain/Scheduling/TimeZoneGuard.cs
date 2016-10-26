using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITimeZoneGuard
	{
		TimeZoneInfo CurrentTimeZone();
		TimeZoneInfo TimeZone { get; set; }
	}
	
	public class TimeZoneGuard : ITimeZoneGuard
	{
		private TimeZoneInfo _timeZone;

		public TimeZoneInfo TimeZone
		{
			get { return _timeZone ?? (_timeZone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone); }
			set { _timeZone = value; }
		}

		public static ITimeZoneGuard Instance => ServiceLocatorForLegacy.TimeZoneGuard;

		public TimeZoneInfo CurrentTimeZone()
		{
			return TimeZone;
		}
	}
}