using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public class PublishRecurringEvents : INavigationInterceptor
	{
		public void Before(GotoArgs args)
		{
			DataMaker.PublishRecurringEvents();
		}

		public void After(GotoArgs args)
		{
		}
	}
}