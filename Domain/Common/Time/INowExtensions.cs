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

		//public static DateTime UserDateTime()
		//{
		//	var userTimeZone = new UserTimeZone(new LoggedOnUser())
		//	if (_fakedUtcDateTime.HasValue)
		//	{
		//		return _userTimeZone().TimeZone() == null ?
		//			_fakedUtcDateTime.Value :
		//			TimeZoneHelper.ConvertToUtc(_fakedUtcDateTime.Value, _userTimeZone().TimeZone());
		//	}
		//	return DateTime.UtcNow;
		//}

		public static DateOnly LocalDateOnly(this INow now)
		{
			return new DateOnly(now.LocalDateTime());
		}

		public static DateOnly UtcDateOnly(this INow now)
		{
			return new DateOnly(now.UtcDateTime());
		}
	}
}