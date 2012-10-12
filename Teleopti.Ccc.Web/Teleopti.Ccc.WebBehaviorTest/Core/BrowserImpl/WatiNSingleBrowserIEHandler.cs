using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl
{
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class WatiNSingleBrowserIEHandler : IBrowserHandler<IE>
	{
		private const string ProcessName = "iexplore";

		private static IDisposable BrowserLock { get; set; }
		//private static IDisposable BrowserLock { get { return ScenarioContext.Current.Value<SystemLevelLock>(); } set { ScenarioContext.Current.Value((SystemLevelLock)value); } }

		//private static bool _closeByWatiNCloseNDisposeFailed = false;
		private IE _browser;

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public IE Start()
		{
			LockBrowser();
			Settings.AutoCloseDialogs = true;
			Settings.AutoMoveMousePointerToTopLeft = false;
			Settings.HighLightColor = "Green";
			Settings.HighLightElement = true;
			Settings.MakeNewIe8InstanceNoMerge = true;
			Settings.MakeNewIeInstanceVisible = true;
			return StartBrowser();
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private IE StartBrowser()
		{
			_browser = new IE {AutoClose = true};
			_browser.ClearCache();
			_browser.ClearCookies();
			_browser.BringToFront();
			return _browser;
		}

		public void PrepareForTestRun() { MakeSureBrowserIsNotRunning(); }

		public void Close()
		{
			try
			{
				CloseBrowser();
			}
			finally
			{
				_browser = null;
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

		public IE Restart()
		{
			CloseBrowser();
			return StartBrowser();
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