using System;

namespace Teleopti.Ccc.TestCommon.Web.StartWeb.BrowserDriver
{
	public interface IBrowserActivator
	{
		void SetTimeout(TimeSpan timeout);
		void Start(TimeSpan timeout, TimeSpan retry);
		IBrowserInteractions GetInteractions();
		void Close();
	}
}