namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public class WaitUntilReadyForInteraction : INavigationInterceptor
	{
		public void Before(GotoArgs args)
		{
		}

		public void After(GotoArgs args)
		{
			TestControllerMethods.WaitUntilReadyForInteraction();
		}
	}
}