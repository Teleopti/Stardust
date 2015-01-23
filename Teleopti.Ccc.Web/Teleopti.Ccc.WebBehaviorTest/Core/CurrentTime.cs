using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public class CurrentTime
	{
		private static DateTime? _currentTime;

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
			fakeServerTime();
			if (FeatureContext.Current.FeatureInfo.Title.StartsWith("Real time adherence"))
				PhoneStateStepDefinitions.CheckForActivityChange();
			fakeClientTime();
		}

		public static void NavigatedToAnApplicationPage()
		{
			if (!_currentTime.HasValue)
				return;
			fakeClientTime();
		}

		private static void fakeServerTime()
		{
			TestControllerMethods.SetCurrentTime(Value());
			//if (FeatureContext.Current.FeatureInfo.Title.StartsWith("Real time adherence"))
			//	PhoneStateStepDefinitions.CheckForActivityChange();
		}

		private static void fakeClientTime()
		{
			if (Browser.Interactions.UrlContains("/Anywhere#realtimeadherenceagents"))	// THIS METHOD IS WRONG IN MANY WAYS
				new FakeClientTimeForAllJsDateObjectsCreatedAsUtcSoTheActualTimeFromGetTimeVaryDependengingOnBrowserTimeZone().Fake(Value());
			else if (Browser.Interactions.UrlContains("/Anywhere#"))
				new FakeClientTimeUsingSinonKnownWorkableWay().Fake(Value());
			else if (Browser.Interactions.UrlContains("/MyTime"))
				new FakeTimeUsingMyTimeAsmMethod().Fake(Value());
		}
	}




	public class FakeClientTimeUsingSinonKnownWorkableWay
	{
		public void Fake(DateTime time)
		{
			const string fakeTime = @"window.fakeTime({0}, {1}, {2}, {3}, {4}, {5});";
			var fakeTimeScript = string.Format(fakeTime, time.Year, time.Month - 1, time.Day, time.Hour, time.Minute, time.Second);
			Browser.Interactions.Javascript(fakeTimeScript);
		}
	}

	public class FakeClientTimeForAllJsDateObjectsCreatedAsUtcSoTheActualTimeFromGetTimeVaryDependengingOnBrowserTimeZone
	{
		public void Fake(DateTime time)
		{
			const string setJsDateTemplate =
				@"Date.prototype.getTime = function () {{ return new Date(Date.UTC({0}, {1}, {2}, {3}, {4}, {5})); }};";
			var setJsDate = string.Format(setJsDateTemplate, time.Year, time.Month - 1, time.Day, time.Hour, time.Minute,
				time.Second);
			Browser.Interactions.Javascript(setJsDate);
		}
	}

	public class FakeTimeUsingMyTimeAsmMethod
	{
		public void Fake(DateTime time)
		{
			const string setJsDateTemplate =
				@"Date.prototype.getTeleoptiTime = function () {{ return new Date({0}, {1}, {2}, {3}, {4}, {5}).getTime(); }};";
			var setJsDate = string.Format(setJsDateTemplate, time.Year, time.Month - 1, time.Day, time.Hour, time.Minute,
				time.Second);

			Browser.Interactions.Javascript(setJsDate);

			var setJsTimeIndicatorMovement =
				string.Format(@"Teleopti.MyTimeWeb.Schedule.SetTimeIndicator(new Date({0}, {1}, {2}, {3}, {4}, {5}));",
					time.Year, time.Month - 1, time.Day, time.Hour, time.Minute, time.Second);
			Browser.Interactions.Javascript(setJsTimeIndicatorMovement);
		}

	}
	
}
