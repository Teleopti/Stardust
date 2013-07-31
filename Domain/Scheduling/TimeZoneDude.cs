using System;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITimeZoneGuard
	{
		TimeZoneInfo TimeZone { get; set; }
	}

	public sealed class TimeZoneGuard : ITimeZoneGuard
	{
		public TimeZoneInfo TimeZone { get; set; }
		private static volatile TimeZoneGuard _guard;
		private static readonly object Locker = new Object();

		private TimeZoneGuard() { }

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
	}
}