namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl
{
	public interface IBrowserHandler<T>
	{
		T Internal { get; }
		void Start();
		void Close();
		void NotifyBeforeTestRun();
		void NotifyBeforeScenario();
	}
}