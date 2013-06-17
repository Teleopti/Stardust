using System;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.CoypuIE;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.WatiNIE;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class Browser
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (Browser));
		private static readonly IBrowserActivator BrowserActivator = new CoypuBrowserActivator();

		public static IE Current
		{
			get
			{
				var activator = BrowserActivator as WatiNSingleBrowserIEActivator;
				return activator != null ? activator.Internal : null;
			}
		}

		public static IBrowserInteractions Interactions { get { return BrowserActivator.GetInteractions(); } }

		public static void Start()
		{
			BrowserActivator.Start();
		}

		public static bool IsStarted()
		{
			return BrowserActivator.IsRunning();
		}

		public static void NotifyBeforeTestRun()
		{
			BrowserActivator.NotifyBeforeTestRun();
		}

		public static void NotifyBeforeScenario()
		{
			BrowserActivator.NotifyBeforeScenario();
		}

		public static void Close()
		{
			Log.Write("Closing the browser");
			BrowserActivator.Close();
		}

	}
}
