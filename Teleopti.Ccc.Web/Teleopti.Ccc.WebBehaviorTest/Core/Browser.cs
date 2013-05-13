﻿using Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserInteractions;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserInteractions.WatiNIE;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class Browser
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (Browser));
		private static readonly IBrowserActivator<IE> BrowserActivator = new WatiNSingleBrowserIEActivator();

		public static IE Current { get { return BrowserActivator.Internal; } }

		public static IBrowserInteractions Interactions { get { return new IEWatiNBrowserInteractions(BrowserActivator.Internal); } }

		public static void Start()
		{
			BrowserActivator.Start();
		}

		public static bool IsStarted()
		{
			return BrowserActivator.Internal != null;
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
