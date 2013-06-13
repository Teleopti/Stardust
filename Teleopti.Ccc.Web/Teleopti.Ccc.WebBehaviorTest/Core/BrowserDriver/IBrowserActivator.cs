namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public interface IBrowserActivator<T>
	{
		T Internal { get; }
		void Start();
		void Close();
		void NotifyBeforeTestRun();
		void NotifyBeforeScenario();
		IBrowserInteractions GetInteractions();
	}
}