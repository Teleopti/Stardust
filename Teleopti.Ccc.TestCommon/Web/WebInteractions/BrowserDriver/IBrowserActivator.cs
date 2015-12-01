using System;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver
{
	public interface IBrowserActivator : IDisposable
	{
		void SpecialTimeout(TimeSpan? timeout);

		void Start(TimeSpan timeout, TimeSpan retry);
		IBrowserInteractions GetInteractions();
	}
}