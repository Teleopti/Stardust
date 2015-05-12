using System;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public class FakeClientTimeUsingSinonProvenWay : INavigationInterceptor, IFakeClientTimeMethod
	{
		public void Before(GotoArgs args)
		{
		}

		public void After(GotoArgs args)
		{
			Apply();
		}

		public void Apply()
		{
			if (!CurrentTime.IsFaked())
				return;
			var time = CurrentTime.Value();
			var browserLocalTime = TimeZoneInfo.ConvertTimeFromUtc(time, TimeZoneInfo.Local);
			const string fakeTime = @"window.fakeTime({0}, {1}, {2}, {3}, {4}, {5});";
			var fakeTimeScript = string.Format(fakeTime, browserLocalTime.Year, browserLocalTime.Month - 1, browserLocalTime.Day, browserLocalTime.Hour, browserLocalTime.Minute, browserLocalTime.Second);
			Browser.Interactions.Javascript(fakeTimeScript);
		}
	}
}