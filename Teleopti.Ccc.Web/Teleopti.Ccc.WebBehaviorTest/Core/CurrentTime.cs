using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class CurrentTime
	{
		private static DateTime? _currentTime;

		public static bool IsFaked()
		{
			return _currentTime.HasValue;
		}

		public static DateTime Value()
		{
			return _currentTime ?? DateTime.UtcNow;
		}

		public static void Reset()
		{
			_currentTime = null;
			LocalSystem.Now.Reset();
		}

		public static void Set(string timeOrDate)
		{
			_currentTime = MagicParse(timeOrDate);
			LocalSystem.Now.Is(_currentTime);
			TestControllerMethods.SetCurrentTime(Value());
			Navigation.Navigation.ReapplyFakeTime();
		}

		public static DateTime MagicParse(string timeOrDate)
		{
			if (timeOrDate.Contains(" ")) // full date time detected
				return timeOrDate.Utc();
			if (timeOrDate.Contains("-")) // probably only a date
				return timeOrDate.Utc();
			if (timeOrDate.Contains(":")) // possibly just a time
				return $"{Value().ToShortDateString()} {timeOrDate}".Utc();
			return timeOrDate.Utc(); // ah wth...
		}
	}
}
