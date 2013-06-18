﻿using Coypu;
using Coypu.Drivers.Selenium;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuImpl
{
	public class CoypuBrowserActivator : IBrowserActivator
	{
		private BrowserSession _browser;

		public void Start()
		{
			var sessionConfiguration = new SessionConfiguration
				{
					AppHost = "about:blank",
					Port = 80,
					SSL = false,
					Driver = typeof(SeleniumWebDriver),
					//Browser = Coypu.Drivers.Browser.InternetExplorer,
					//Browser = Coypu.Drivers.Browser.Firefox,
					Browser = Coypu.Drivers.Browser.Chrome
				};
			_browser = new BrowserSession(sessionConfiguration);
		}

		public bool IsRunning()
		{
			return _browser != null;
		}

		public void Close()
		{
			if (_browser == null) return;
			_browser.Dispose();
			_browser = null;
		}

		public void NotifyBeforeTestRun()
		{
		}

		public void NotifyBeforeScenario()
		{
		}

		public IBrowserInteractions GetInteractions()
		{
			return new CoypuBrowserInteractions(_browser);
		}
	}
}
