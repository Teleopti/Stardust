using System;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITimeZoneGuard
	{
		TimeZoneInfo TimeZone { get; set; }
	}

	public sealed class TimeZoneGuard : ITimeZoneGuard
	{
		private static readonly Lazy<TimeZoneGuard> _guard = new Lazy<TimeZoneGuard>(()=>new TimeZoneGuard());
		
		private TimeZoneGuard()
		{
			TimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
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