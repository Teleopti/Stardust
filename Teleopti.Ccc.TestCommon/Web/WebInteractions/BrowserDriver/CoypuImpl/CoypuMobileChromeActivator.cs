using System;
using Coypu.Drivers.Selenium;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver.CoypuImpl
{
	public class CoypuMobileChromeActivator : CoypuBrowserActivator
	{
		public CoypuMobileChromeActivator() : base(typeof(ChromeWebDriverWithMobileProfile), Coypu.Drivers.Browser.Chrome)
		{
		}
	}

	public class ChromeWebDriverWithMobileProfile : SeleniumWebDriver
	{
		public ChromeWebDriverWithMobileProfile(Coypu.Drivers.Browser browser)
			: base(MobileProfileChromeDriver(), browser)
		{
		}

		private static ChromeDriver MobileProfileChromeDriver()
		{
			var profilePath = System.IO.Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ".ChromeWebDriverProfile");
			var options = new ChromeOptions();

			var iPhone6UserAgentStr = "Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1";
			options.AddArgument($"--user-data-dir={profilePath}");
			options.AddArgument($"--user-agent={iPhone6UserAgentStr}");
			options.SetLoggingPreference(LogType.Browser, LogLevel.All);

			return new ChromeDriver(options);
		}
	}
}