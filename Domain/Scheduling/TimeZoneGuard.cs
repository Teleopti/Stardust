using System;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITimeZoneGuard
	{
		TimeZoneInfo CurrentTimeZone();
	}

	public class TimeZoneGuardWrapper : ITimeZoneGuard
	{
		public TimeZoneInfo CurrentTimeZone()
		{
			return TimeZoneGuard.Instance.TimeZone;
		}
	}

	public sealed class TimeZoneGuard
	{
		private static readonly Lazy<TimeZoneGuard> _guard = new Lazy<TimeZoneGuard>(()=>new TimeZoneGuard());
		
		private TimeZoneGuard()
		{
			TimeZone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;
		}

		public static TimeZoneGuard Instance
		{
			get
			{
				return _guard.Value;
			}
		}

		public TimeZoneInfo TimeZone { get; set; }
	}
}