using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class Browser
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (Browser));

		private static readonly IBrowserHandler<IE> BrowserHandler = new IEHandler();

		private static IE ScenarioBrowser { get { return ScenarioContext.Current.Value<IE>(); } set { ScenarioContext.Current.Value(value); } }

		public static IE Current
		{
			get
			{
				if (!IsStarted())
					Start();
				return ScenarioBrowser;
			}
		}

		private static void Start() { ScenarioBrowser = BrowserHandler.Start(); }

		public static bool IsStarted() { return ScenarioBrowser != null; }

		public static void PrepareForTestRun() { BrowserHandler.PrepareForTestRun(); }

		public static void Close()
		{
			var browser = ScenarioBrowser;
			ScenarioBrowser = null;
			Log.Write("Closing the browser");
			BrowserHandler.Close(browser);
		}

	}
}
