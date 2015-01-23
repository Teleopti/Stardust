namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public class WaitUntilCompletelyLoaded : INavigationInterceptor
	{
		public void Before(GotoArgs args)
		{
		}

		public void After(GotoArgs args)
		{
			TestControllerMethods.WaitUntilCompletelyLoaded();
		}
	}
}