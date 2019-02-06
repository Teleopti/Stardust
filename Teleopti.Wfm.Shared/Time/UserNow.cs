using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public class UserNow : IUserNow
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;

		public UserNow(INow now, IUserTimeZone timeZone)
		{
			_now = now;
			_timeZone = timeZone;
		}
		
		public DateTime DateTime()
		{
			return TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
		}
	}
}