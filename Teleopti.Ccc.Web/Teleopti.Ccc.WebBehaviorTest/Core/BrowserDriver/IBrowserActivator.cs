using System;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public interface IBrowserActivator
	{
		void SetTimeout(TimeSpan timeout);
		void Start(TimeSpan timeout, TimeSpan retry);
		bool IsRunning();
		void Close();
		void NotifyBeforeTestRun();
		void NotifyBeforeScenario();
		IBrowserInteractions GetInteractions();
	}
}