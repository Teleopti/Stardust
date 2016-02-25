using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public class WaitUntilHangfireQueueIsProcessed : INavigationInterceptor
	{
		public void Before(GotoArgs args)
		{
			SystemSetup.Hangfire.WaitForQueue();
		}

		public void After(GotoArgs args)
		{
		}
	}
}