namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public class DONTUSE_FakeClientTimeForAllJsDateObjectsCreatedAsUtcSoTheActualTimeFromGetTimeVaryDependengingOnBrowserTimeZone : INavigationInterceptor, IFakeClientTimeMethod
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
			const string setJsDateTemplate =
				@"Date.prototype.getTime = function () {{ return new Date(Date.UTC({0}, {1}, {2}, {3}, {4}, {5})); }};";
			var setJsDate = string.Format(setJsDateTemplate, time.Year, time.Month - 1, time.Day, time.Hour, time.Minute,
				time.Second);
			Browser.Interactions.Javascript(setJsDate);
		}
	}
}