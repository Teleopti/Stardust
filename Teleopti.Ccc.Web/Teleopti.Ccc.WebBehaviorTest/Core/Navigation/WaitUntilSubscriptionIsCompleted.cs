namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public class WaitUntilSubscriptionIsCompleted : INavigationInterceptor
	{
		public void Before(GotoArgs args)
		{
		}

		public void After(GotoArgs args)
		{
			Browser.Interactions.AssertExists("[data-subscription-done]");
		}
	}
}