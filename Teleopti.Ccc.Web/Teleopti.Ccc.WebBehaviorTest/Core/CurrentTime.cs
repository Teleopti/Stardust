using System;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public class CurrentTime
	{
		private static DateTime _currentTime;

		public static DateTime Value()
		{
			return _currentTime;
		}

		public static void Set(DateTime time)
		{
			_currentTime = time;
			Navigation.GoToWaitForCompleted("Test/SetCurrentTime?ticks=" + time.Ticks);
		}
	}
}