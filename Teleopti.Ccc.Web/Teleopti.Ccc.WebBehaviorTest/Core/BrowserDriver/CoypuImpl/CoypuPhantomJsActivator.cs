using Coypu.Drivers.Selenium;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuImpl
{
	public class CoypuPhantomJsActivator : CoypuBrowserActivator
	{
		public CoypuPhantomJsActivator() : base(typeof(SeleniumWebDriver), Coypu.Drivers.Browser.PhantomJS)
		{
		}
	}
}