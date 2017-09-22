namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public class WaitUntilLoaded : INavigationInterceptor
	{
		public void After(GotoArgs args)
		{
			Browser.Interactions.AssertExists(".wfm-list");
		}

		public void Before(GotoArgs args)
		{
			
		}
	}
}