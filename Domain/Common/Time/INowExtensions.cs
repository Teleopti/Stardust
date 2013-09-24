using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public static class INowExtensions
	{
		public static DateTime LocalDateTime(this INow now)
		{
			return now.UtcDateTime().ToLocalTime();
		}

		public static DateOnly LocalDateOnly(this INow now)
		{
			return new DateOnly(now.LocalDateTime());
		}
	}
}