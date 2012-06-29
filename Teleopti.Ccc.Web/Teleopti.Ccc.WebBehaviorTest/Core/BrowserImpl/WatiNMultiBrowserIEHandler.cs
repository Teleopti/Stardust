using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl
{
	public class WatiNMultiBrowserIEHandler : IBrowserHandler<IE>
	{
		private const string ProcessName = "iexplore";

		private static readonly ILog Log = LogManager.GetLogger(typeof(WatiNSingleBrowserIEHandler));

		private IE browser;
		private IntPtr browserWindowHandle;

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public IE Start()
		{
			Settings.AutoCloseDialogs = true;
			Settings.AutoMoveMousePointerToTopLeft = false;
			Settings.HighLightColor = "Green";
			Settings.HighLightElement = true;
			Settings.MakeNewIe8InstanceNoMerge = true;
			Settings.MakeNewIeInstanceVisible = true;
			browser = new IE();
			browser.ClearCache();
			browser.ClearCookies();
			browser.BringToFront();
			browserWindowHandle = browser.hWnd; // because if the close method is called in AfterTestRun ForceClose or hWnd doesnt work any more
			return browser;
		}

		public void PrepareForTestRun() { }

		public void Close()
		{
			var startTime = DateTime.Now;
			//CloseByWatiNCloseNDispose(browser);
			BrowserProcessHelpers.CloseByClosingMainWindow(ProcessName, browserWindowHandle);
			//CloseByWatiNForceClose(browser);
			Log.Write("Close took " + DateTime.Now.Subtract(startTime));
		}

	}
}