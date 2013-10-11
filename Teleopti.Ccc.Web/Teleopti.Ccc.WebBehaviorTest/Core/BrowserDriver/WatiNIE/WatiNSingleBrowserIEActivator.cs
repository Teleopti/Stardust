using System;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.WatiNIE
{
	public class WatiNSingleBrowserIEActivator : IBrowserActivator
	{
		private const string ProcessName = "iexplore";

		private static IDisposable BrowserLock { get; set; }

		private static int _scenarioCount = 0;

		private static void ResetScenarioCount() { _scenarioCount = 0; }
		private static void IncrementScenarioCount() { _scenarioCount += 1; }

		public IE Internal { get; set; }
		private WatiNIEBrowserInteractions _interactions;

		public void SetTimeout(TimeSpan timeout)
		{
			Settings.WaitForCompleteTimeOut = Convert.ToInt32(timeout.TotalSeconds);
			Settings.WaitUntilExistsTimeOut = Convert.ToInt32(timeout.TotalSeconds);
		}

		public void Start(TimeSpan timeout, TimeSpan retry)
		{
			MakeSureBrowserIsNotRunning();
			ResetScenarioCount();

			LockBrowser();
			Settings.AutoCloseDialogs = true;
			Settings.AutoMoveMousePointerToTopLeft = false;
			Settings.HighLightColor = "Green";
			Settings.HighLightElement = true;
			Settings.MakeNewIe8InstanceNoMerge = true;
			Settings.MakeNewIeInstanceVisible = true;
			SetTimeout(timeout);
			StartBrowser();
		}

		private void StartBrowser()
		{
			Internal = new IE { AutoClose = true };
			// Clear the browser cache this way to solve caching issues that occurr on the build server sometimes?
			// Process.Start("RunDll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 8");
			Internal.ClearCache();
			Internal.ClearCookies();
			Internal.BringToFront();
			_interactions = new WatiNIEBrowserInteractions(Internal);
		}

		public void Close()
		{
			if (Internal == null)
				return;
			try
			{
				CloseBrowser();
			}
			finally
			{
				Internal = null;
				_interactions = null;
				ReleaseBrowser();
			}
		}

		private void CloseBrowser()
		{
			var result = ProcessHelpers.TryToCloseProcess(
				ProcessName,
				new Func<TryResult>[]
					{
						// never works in AfterTestRun
						//() => TryCloseByWatiNCloseNDispose(),
						() => ProcessHelpers.TryCloseByClosingMainWindow(ProcessName),
						// never works in AfterTestRun
						//() => TryCloseByWatiNForceClose(),
						() => ProcessHelpers.TryCloseByKillingProcess(ProcessName)
					});
			if (!result)
				throw new ApplicationException("Browser failed to close.");
		}

		public void NotifyBeforeScenario()
		{
			// restart browser every 15th scenario
			if (_scenarioCount != 0 && _scenarioCount%15 == 0)
			{
				CloseBrowser();
				StartBrowser();
			}
			IncrementScenarioCount();
		}

		public IBrowserInteractions GetInteractions()
		{
			return _interactions;
		}


		private void MakeSureBrowserIsNotRunning()
		{
			using (MakeBrowserLock())
			{
				var result = ProcessHelpers.TryToCloseProcess(
					ProcessName,
					new Func<TryResult>[]
						{
							() => ProcessHelpers.TryCloseByClosingMainWindow(ProcessName),
							() => ProcessHelpers.TryCloseByKillingProcess(ProcessName)
						});
				if (!result)
					throw new ApplicationException("Browser failed to close when making sure it isnt running");
			}
		}



		//private TryResult TryCloseByWatiNForceClose()
		//{
		//    _browser.ForceClose();
		//    return TryResult.Passed;
		//}

		//private TryResult TryCloseByWatiNCloseNDispose()
		//{
		//    if (_closeByWatiNCloseNDisposeFailed)
		//        return TryResult.Failure;
		//    var success = Task.Factory
		//        .StartNew(() =>
		//                    {
		//                        _browser.Close();
		//                        _browser.Dispose();
		//                    })
		//        .Wait(TimeSpan.FromSeconds(2));
		//    if (!success)
		//    {
		//        _closeByWatiNCloseNDisposeFailed = true;
		//        return TryResult.Failure;
		//    }
		//    return TryResult.Passed;
		//}




		private static SystemLevelLock MakeBrowserLock() { return new SystemLevelLock("TestBrowserLock"); }

		private static void LockBrowser() { BrowserLock = MakeBrowserLock(); }

		private static void ReleaseBrowser()
		{
			BrowserLock.Dispose();
			BrowserLock = null;
		}

	}
}