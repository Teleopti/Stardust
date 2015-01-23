namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public interface INavigationInterceptor
	{
		void Before(GotoArgs args);
		void After(GotoArgs args);
	}
}