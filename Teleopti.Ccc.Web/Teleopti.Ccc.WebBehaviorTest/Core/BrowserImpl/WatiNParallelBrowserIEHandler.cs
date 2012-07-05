using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon;
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

		private IE _browser;

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public IE Start()
		{
			Settings.AutoCloseDialogs = true;
			Settings.AutoMoveMousePointerToTopLeft = false;
			Settings.HighLightColor = "Green";
			Settings.HighLightElement = true;
			Settings.MakeNewIe8InstanceNoMerge = true;
			Settings.MakeNewIeInstanceVisible = true;

			_browser = new IE { AutoClose = true };
			_browser.ClearCache();
			_browser.ClearCookies();
			_browser.BringToFront();

			return _browser;
		}

		//private GenericDisposable TrackProcessId()
		//{
		//    // this simple way does not work in a cc.net service because we cant get window handle because there's no actual explorer window
		//    //processId = ProcessHelpers.ProcessIdForMainWindow(ProcessName, browserWindowHandle);
		//    var originalProcesses = (from p in Process.GetProcessesByName(ProcessName) select p.Id).ToArray();
		//    return new GenericDisposable(() =>
		//                                    {
		//                                        var processes = Process.GetProcessesByName(ProcessName).AsEnumerable();
		//                                        processIds = (from p in processes
		//                                                      let id = p.Id
		//                                                      let isNewProcess = !originalProcesses.Contains(id)
		//                                                      //let isTab = p.MainWindowTitle != 
		//                                                      where isNewProcess
		//                                                      select id).ToArray();
		//                                        if (!processes.Any())
		//                                            throw new Exception("Process tracking failed!");
		//                                    });
		//}

		//private int SelectProcessId()
		//{
		//    var processes = from id in processIds
		//                    select Process.GetProcessById(id);
		//    var processThatUsedMostMemory = processes.OrderByDescending(p => p.PrivateMemorySize64).First();
		//    return processThatUsedMostMemory.Id;
		//}

		//private void RetrieveWindowHandleSafely()
		//{
		//    // because if the close method is called in AfterTestRun ForceClose or hWnd doesnt work any more
		//    // same goes for retrieving the window handle in AfterTestRun
		//    windowHandle = _browser.hWnd;
		//}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public void PrepareForTestRun()
		{
			using (new SystemLevelLock("TestBrowserCleaningLock"))
			{
				var nunitProcesses = Process.GetProcessesByName("nunit-console");
				if (nunitProcesses.Count() > 1)
					return;

				ProcessHelpers.TryToCloseProcess(
					ProcessName,
					new Func<TryResult>[]
						{
							() => ProcessHelpers.TryCloseByClosingMainWindow(ProcessName),
							() => ProcessHelpers.TryCloseByKillingProcess(ProcessName)
						});

			}
		}

		public void Close()
		{
			var startTime = DateTime.Now;

			_browser.Close();
			_browser.Dispose();
			_browser = null;

			Log.Write("Close took " + DateTime.Now.Subtract(startTime));
		}

	}
}