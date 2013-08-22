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
		private static volatile TimeZoneGuard _guard;
		private static readonly object Locker = new Object();

		private TimeZoneGuard()
		{
			TimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
		}

		public static TimeZoneGuard Instance
		{
			get
			{
				if (_guard == null)
				{
					lock (Locker)
					{
						if (_guard == null)
							_guard = new TimeZoneGuard();
					}
				}

				return _guard;
			}
		}

		public TimeZoneInfo TimeZone { get; set; }
	}
}