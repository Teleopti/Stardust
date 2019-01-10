using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public static class INowExtensions
	{
		/// <summary>
		/// when ever(!) is the servers timezone a good idea to use?
		/// </summary>
		/// <param name="now"></param>
		/// <returns>a probably incorrect value for your use</returns>
		public static DateTime ServerDateTime_DontUse(this INow now)
		{
			return now.UtcDateTime().ToLocalTime();
		}

		public static DateTime CurrentLocalDateTime(this INow now, TimeZoneInfo timeZoneInfo)
		{
			return TimeZoneHelper.ConvertFromUtc(now.UtcDateTime(), timeZoneInfo);
		}

		public static DateOnly CurrentLocalDate(this INow now, TimeZoneInfo timeZoneInfo)
		{
			return now.CurrentLocalDateTime(timeZoneInfo).ToDateOnly();
		}

		/// <summary>
		/// when ever(!) is the servers timezone a good idea to use?
		/// </summary>
		/// <param name="now"></param>
		/// <returns>a probably incorrect value for your use</returns>
		public static DateOnly ServerDate_DontUse(this INow now)
		{
			return new DateOnly(now.ServerDateTime_DontUse());
		}
	}
}