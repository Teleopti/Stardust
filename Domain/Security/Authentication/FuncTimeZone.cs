using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class FuncTimeZone : IUserTimeZone
	{
		private readonly Func<TimeZoneInfo> _timeZone;

		public FuncTimeZone(Func<TimeZoneInfo> timeZone)
		{
			_timeZone = timeZone;
		}

		public TimeZoneInfo TimeZone()
		{
			return _timeZone.Invoke();
		}
	}
}