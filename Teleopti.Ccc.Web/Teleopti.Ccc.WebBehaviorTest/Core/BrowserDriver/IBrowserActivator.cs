using System;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	public interface IBrowserActivator
	{
		void SetTimeout(TimeSpan timeout);
		void Start(TimeSpan timeout, TimeSpan retry, string host, int port);
		IBrowserInteractions GetInteractions();
		void Close();
	}
}