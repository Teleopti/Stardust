using System;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITimeZoneDude
	{
		TimeZoneInfo TimeZone { get; set; }
	}

	public sealed class TimeZoneDude : ITimeZoneDude
	{
		public TimeZoneInfo TimeZone { get; set; }
		private static volatile TimeZoneDude _dude;
		private static readonly object Locker = new Object();

		private TimeZoneDude() { }

		public static TimeZoneDude Instance
		{
			get
			{
				if (_dude == null)
				{
					lock (Locker)
					{
						if (_dude == null)
							_dude = new TimeZoneDude();
					}
				}

				return _dude;
			}
		}
	}
}