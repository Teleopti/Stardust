using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public class CurrentTime
	{
		private static DateTime? _currentTime;

		public static bool IsFaked()
		{
			return _currentTime.HasValue;
		}

		public static DateTime Value()
		{
			return _currentTime.HasValue ? _currentTime.Value : DateTime.UtcNow;
		}

		public static void Reset()
		{
			_currentTime = null;
			LocalSystem.Now.Reset();
		}

		public static void Set(string time)
		{
			_currentTime = time.Utc();
			LocalSystem.Now.Is(_currentTime);
			TestControllerMethods.SetCurrentTime(Value());
			Navigation.Navigation.ReapplyFakeTime();
		}
	}
}
