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

			const string fakeTime = @"window.fakeTime({0}, {1}, {2}, {3}, {4}, {5});";
			var fakeTimeScript = string.Format(fakeTime, time.Year, time.Month - 1, time.Day, time.Hour, time.Minute, time.Second);
			Browser.Interactions.Javascript(fakeTimeScript);
		}
	}
}