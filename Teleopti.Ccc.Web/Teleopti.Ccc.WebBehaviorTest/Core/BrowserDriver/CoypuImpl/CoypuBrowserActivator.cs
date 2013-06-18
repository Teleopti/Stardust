using System;
using Coypu;
using Coypu.Drivers.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

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
					Driver = typeof(ChromeWebDriverWithProfile),
					//Driver = typeof(SeleniumWebDriver),
					//Browser = Coypu.Drivers.Browser.InternetExplorer,
					//Browser = Coypu.Drivers.Browser.Firefox,
					//Browser = Coypu.Drivers.Browser.Chrome
				};
			// to get chrome to be fast, disable automatic detection of proxy settings. odd, but it works:
			// http://stackoverflow.com/questions/16179808/chromedriver-is-extremely-slow-on-a-specific-machine-using-selenium-grid-and-ne
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

	public class ChromeWebDriverWithProfile : SeleniumWebDriver
	{
		public ChromeWebDriverWithProfile(Coypu.Drivers.Browser browser)
			: base(CustomProfileDriver(), browser)
		{
		}

		private static ChromeDriver CustomProfileDriver()
		{
			var profilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "ChromeWebDriverProfile");
			var options = new ChromeOptions();
			options.AddArgument(string.Format("--user-data-dir={0}", profilePath));
			return new ChromeDriver(options);
		}
	}

}
