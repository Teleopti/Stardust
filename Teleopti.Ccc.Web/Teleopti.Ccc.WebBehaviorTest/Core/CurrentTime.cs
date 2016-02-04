using System;
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
			SystemSetup.Now.Reset();
		}

		public static void Set(DateTime time)
		{
			_currentTime = time;
			SystemSetup.Now.Is(time);
			TestControllerMethods.SetCurrentTime(Value());
			Navigation.Navigation.ReapplyFakeTime();
		}
	}
}
