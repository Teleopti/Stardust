using Coypu.Drivers.Selenium;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver.CoypuImpl
{
	public class CoypuPhantomJsActivator : CoypuBrowserActivator
	{
		public CoypuPhantomJsActivator() : base(typeof(SeleniumWebDriver), Coypu.Drivers.Browser.PhantomJS)
		{
		}
	}
}