using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
			
			browserWindowHandle = browser.hWnd;

			Func<bool> tryToGetProcessId = () =>
			                              	{
			                              		try
			                              		{
													Log.Write("browserWindowHandle " + browserWindowHandle);
			                              			var processes = Process.GetProcessesByName(ProcessName).ToList();
			                              			processes.ForEach(p =>
			                              			                  	{
																			Log.Write("p.MainWindowHandle " + p.MainWindowHandle);
			                              			                  	});
													processId = ProcessHelpers.ProcessIdForMainWindow(ProcessName, browserWindowHandle);
			                              			return true;
			                              		}
			                              		catch (ProcessNotFoundException)
			                              		{
			                              			return false;
			                              		}
			                              	};
			tryToGetProcessId.WaitOrThrow(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(5));

			return browser;
		}

		public void PrepareForTestRun()
		{
			var startTime = DateTime.Now;

			var result = ProcessHelpers.TryToCloseProcess(
				ProcessName,
				new Func<string, bool>[]
					{
						ProcessHelpers.TryCloseByClosingMainWindow,
						ProcessHelpers.TryCloseByKillingProcess
					});
			if (!result)
				throw new ApplicationException("Browser failed to close when making sure it isnt running");

			Log.Write("MakeSureBrowserIsNotRunning " + DateTime.Now.Subtract(startTime));
		}

		public void Close()
		{
			var startTime = DateTime.Now;
			// because if the close method is called in AfterTestRun ForceClose or hWnd doesnt work any more
			// same goes for retrieving the window handle at this stage
			ProcessHelpers.CloseByClosingMainWindow(ProcessName, browserWindowHandle);
			Log.Write("Close took " + DateTime.Now.Subtract(startTime));
		}

	}
}