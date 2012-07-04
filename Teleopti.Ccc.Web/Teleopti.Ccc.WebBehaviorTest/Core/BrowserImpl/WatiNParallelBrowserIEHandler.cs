using System;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class WatiNParallelBrowserIEHandler : IBrowserHandler<IE>
	{
		private const string ProcessName = "iexplore";

		private static readonly ILog Log = LogManager.GetLogger(typeof(WatiNSingleBrowserIEHandler));

		private IE browser;
		private IntPtr browserWindowHandle;
		private int processId;

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
			processId = BrowserProcessHelpers.ProcessIdForMainWindow(ProcessName, browserWindowHandle);

			return browser;
		}

		public void PrepareForTestRun()
		{
			var startTime = DateTime.Now;

			var result = BrowserProcessHelpers.AttemptToCloseProcess(
				ProcessName,
				new Func<bool>[]
					{
						() => BrowserProcessHelpers.CloseByClosingMainWindow(ProcessName),
						() => BrowserProcessHelpers.CloseByKillingProcesses(ProcessName)
					});
			if (!result)
				throw new ApplicationException("Browser failed to close when making sure it isnt running");

			Log.Write("MakeSureBrowserIsNotRunning " + DateTime.Now.Subtract(startTime));
		}

		public void Close()
		{
			var startTime = DateTime.Now;
			BrowserProcessHelpers.CloseByClosingMainWindow(ProcessName, browserWindowHandle);
			Log.Write("Close took " + DateTime.Now.Subtract(startTime));
		}

	}
}