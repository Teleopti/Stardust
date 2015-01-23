using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic;

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
		}

		public static void Set(DateTime time)
		{
			_currentTime = time;
			TestControllerMethods.SetCurrentTime(Value());
			if (FeatureContext.Current.FeatureInfo.Title.StartsWith("Real time adherence"))
				PhoneStateStepDefinitions.CheckForActivityChange();
			Navigation.Navigation.ReapplyFakeTime();
		}
	}
}
