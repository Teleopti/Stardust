namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public interface IBrowserActivator
	{
		void Start();
		bool IsRunning();
		void Close();
		void NotifyBeforeTestRun();
		void NotifyBeforeScenario();
		IBrowserInteractions GetInteractions();
	}
}