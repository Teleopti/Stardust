namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public class WaitUntilSeatPlanPlanningPeriodLoaded:INavigationInterceptor
	{
		public void Before(GotoArgs args)
		{}

		public void After(GotoArgs args)
		{
			Browser.Interactions.AssertExists("[planning-period-loaded]");
		}
	}
}