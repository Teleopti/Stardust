using System;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver
{
	public sealed class TimeoutScope : IDisposable
	{
		private readonly IBrowserActivator _browser;

		public TimeoutScope(IBrowserActivator browser, TimeSpan timeout)
		{
			_browser = browser;
			_browser.SpecialTimeout(timeout);
		}

		public void Dispose()
		{
			_browser.SpecialTimeout(null);
		}
	}
}