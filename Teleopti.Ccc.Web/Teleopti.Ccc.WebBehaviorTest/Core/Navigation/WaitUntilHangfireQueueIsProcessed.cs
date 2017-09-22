using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public class WaitUntilHangfireQueueIsProcessed : INavigationInterceptor
	{
		public void Before(GotoArgs args)
		{
			LocalSystem.Hangfire.WaitForQueue();
		}

		public void After(GotoArgs args)
		{
		}
	}
}