using System;
using Coypu;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuImpl
{
	public class CoypuBrowserActivator : IBrowserActivator
	{
		private readonly Type _driverConfiguration;
		private readonly Coypu.Drivers.Browser _browserConfiguration;
		private BrowserSession _browser;
		private CoypuBrowserInteractions _interactions;

		public CoypuBrowserActivator(Type driverConfiguration, Coypu.Drivers.Browser browserConfiguration)
		{
			_driverConfiguration = driverConfiguration;
			_browserConfiguration = browserConfiguration;
		}

		public void SetTimeout(TimeSpan timeout)
		{
			_interactions.SetTimeout(timeout);
		}

		public void Start(TimeSpan timeout, TimeSpan retry)
		{
			var configuration = new SessionConfiguration
				{
					AppHost = "about:blank",
					Port = 80,
					SSL = false,
					ConsiderInvisibleElements = true,
					WaitBeforeClick = TimeSpan.Zero,
					RetryInterval = retry,
					Timeout = timeout,
					Driver = _driverConfiguration,
					Browser = _browserConfiguration
				};
			_browser = new BrowserSession(configuration);
			_browser.ResizeTo(1200, 1000);	// an element must be in view to selenium to interact with it
			_interactions = new CoypuBrowserInteractions(_browser, configuration);
		}

	    public void Close()
		{
			if (_browser == null) return;
			_browser.Dispose();
			_browser = null;
			_interactions = null;
		}

		public void NotifyBeforeScenario()
		{
		}

		public IBrowserInteractions GetInteractions()
		{
			return _interactions;
		}
	}

}
