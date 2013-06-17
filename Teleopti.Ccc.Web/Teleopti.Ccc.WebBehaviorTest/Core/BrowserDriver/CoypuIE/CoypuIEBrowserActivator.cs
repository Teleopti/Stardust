using System.Collections.Generic;
using System.Linq;
using System.Text;
using Coypu;
using Coypu.Drivers.Selenium;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuIE
{
	public class CoypuIEBrowserActivator : IBrowserActivator<BrowserSession>
	{
		public BrowserSession Internal { get; private set; }

		public void Start()
		{
			var sessionConfiguration = new SessionConfiguration
				{
					AppHost = "about:blank",
					Port = 80,
					SSL = false,
					Driver = typeof(SeleniumWebDriver),
					Browser = Coypu.Drivers.Browser.InternetExplorer
				};
			Internal = new BrowserSession(sessionConfiguration);
		}

		public void Close()
		{
			if (Internal != null)
			{
				Internal.Dispose();
				Internal = null;
			}
		}

		public void NotifyBeforeTestRun()
		{
		}

		public void NotifyBeforeScenario()
		{
		}

		public IBrowserInteractions GetInteractions()
		{
			return new CoypuBrowserInteractions(Internal);
		}
	}
}
