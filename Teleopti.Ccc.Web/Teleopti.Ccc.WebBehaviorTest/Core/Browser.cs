using Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl;
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
		private static readonly IBrowserHandler<IE> BrowserHandler = new WatiNSingleBrowserIEHandler();

		public static IE Current { get { return BrowserHandler.Internal; } }

		public static IBrowserInteractions Interactions { get { return new IEWatiNBrowserInteractions(BrowserHandler.Internal); } }

		public static void Start()
		{
			BrowserHandler.Start();
		}

		public static bool IsStarted()
		{
			return BrowserHandler.Internal != null;
		}

		public static void NotifyBeforeTestRun()
		{
			BrowserHandler.NotifyBeforeTestRun();
		}

		public static void NotifyBeforeScenario()
		{
			BrowserHandler.NotifyBeforeScenario();
		}

		public static void Close()
		{
			Log.Write("Closing the browser");
			BrowserHandler.Close();
		}

	}
}
