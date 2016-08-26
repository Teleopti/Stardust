using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public class EndSetupPhase : INavigationInterceptor
	{
		public void Before(GotoArgs args)
		{
			DataMaker.EndSetupPhase();
		}

		public void After(GotoArgs args)
		{
		}
	}
}