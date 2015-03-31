using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public static class INowExtensions
	{
		/// <summary>
		/// when ever(!) is the servers timezone a good idea to use?
		/// </summary>
		/// <param name="now"></param>
		/// <returns>a probably incorrect value for your use</returns>
		public static DateTime LocalDateTime(this INow now)
		{
			return now.UtcDateTime().ToLocalTime();
		}

		/// <summary>
		/// when ever(!) is the servers timezone a good idea to use?
		/// </summary>
		/// <param name="now"></param>
		/// <returns>a probably incorrect value for your use</returns>
		public static DateOnly LocalDateOnly(this INow now)
		{
			return new DateOnly(now.LocalDateTime());
		}
	}
}