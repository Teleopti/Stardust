using System;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver
{
	public sealed class TimeoutScope : IDisposable
	{
		private readonly IBrowserActivator _browser;
		private readonly TimeSpan _storedTimeout;

		public TimeoutScope(IBrowserActivator browser, TimeSpan timeout)
		{
			_storedTimeout = Timeouts.Timeout;
			Timeouts.Timeout = timeout;
			_browser = browser;
			_browser.SetTimeout(timeout);
		}

		public void Dispose()
		{
			_browser.SetTimeout(_storedTimeout);
			Timeouts.Timeout = _storedTimeout;
		}
	}
}