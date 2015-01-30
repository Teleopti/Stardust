namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public class WaitUntilHangfireQueueIsProcessed : INavigationInterceptor
	{
		public void Before(GotoArgs args)
		{
			TestControllerMethods.WaitForHangfireQueue();
		}

		public void After(GotoArgs args)
		{
		}
	}
}