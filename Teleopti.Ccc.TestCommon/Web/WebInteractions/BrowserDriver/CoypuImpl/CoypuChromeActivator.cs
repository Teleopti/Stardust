using System;
using Coypu.Drivers.Selenium;
using OpenQA.Selenium.Chrome;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver.CoypuImpl
{
	public class CoypuChromeActivator : CoypuBrowserActivator
	{
		// to get chrome to be fast, disable automatic detection of proxy settings. odd, but it works:
		// http://stackoverflow.com/questions/16179808/chromedriver-is-extremely-slow-on-a-specific-machine-using-selenium-grid-and-ne
		// maybe try this: http://stackoverflow.com/questions/5570004/how-to-change-lan-settings-proxy-configuration-programmatically

		public CoypuChromeActivator() : base(typeof(ChromeWebDriverWithNewProfile), Coypu.Drivers.Browser.Chrome)
		{
		}
	}

	public class ChromeWebDriverWithNewProfile : SeleniumWebDriver
	{
		public ChromeWebDriverWithNewProfile(Coypu.Drivers.Browser browser)
			: base(CustomProfileDriver(), browser)
		{
		}

		private static ChromeDriver CustomProfileDriver()
		{
			var profilePath = System.IO.Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ".ChromeWebDriverProfile");
			var options = new ChromeOptions();
			options.AddArgument(string.Format("--user-data-dir={0}", profilePath));
			return new ChromeDriver(options);
		}
	}

}