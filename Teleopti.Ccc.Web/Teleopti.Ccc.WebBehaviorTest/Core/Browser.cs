﻿using Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class Browser
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (Browser));

		private static readonly IBrowserHandler<IE> BrowserHandler = new WatiNSingleBrowserIEHandler();

		private static IE GlobalBrowser { get; set; }

		public static IE Current
		{
			get
			{
				if (!IsStarted())
					Start();
				return GlobalBrowser;
			}
		}

		public static void Start() { GlobalBrowser = BrowserHandler.Start(); }

		public static bool IsStarted() { return GlobalBrowser != null; }

		public static void PrepareForTestRun() { BrowserHandler.PrepareForTestRun(); }

		public static void Close()
		{
			GlobalBrowser = null;
			Log.Write("Closing the browser");
			BrowserHandler.Close();
		}

	}
}
