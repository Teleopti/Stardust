using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl
{
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class WatiNSingleBrowserIEHandler : IBrowserHandler<IE>
	{
		private const string ProcessName = "iexplore";

		private static readonly ILog Log = LogManager.GetLogger(typeof(WatiNSingleBrowserIEHandler));

		private static IDisposable BrowserLock { get { return ScenarioContext.Current.Value<SystemLevelLock>(); } set { ScenarioContext.Current.Value((SystemLevelLock)value); } }

		private static bool _closeByWatiNCloseNDisposeFailed = false;
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
				var result = ProcessHelpers.TryToCloseProcess(
					ProcessName,
					new Func<TryResult>[]
						{
							() => TryCloseByWatiNCloseNDispose(),
							() => ProcessHelpers.TryCloseByClosingMainWindow(ProcessName),
							() => TryCloseByWatiNForceClose(),
							() => ProcessHelpers.TryCloseByKillingProcess(ProcessName)
						});
				if (!result)
					throw new ApplicationException("Browser failed to close.");
			}
			finally
			{
				_browser = null;
				ReleaseBrowser();
			}
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



		private TryResult TryCloseByWatiNForceClose()
		{
			_browser.ForceClose();
			return TryResult.Passed;
		}

		private TryResult TryCloseByWatiNCloseNDispose()
		{
			if (_closeByWatiNCloseNDisposeFailed)
				return TryResult.Failure;
			var success = Task.Factory
				.StartNew(() =>
				          	{
								_browser.Close();
								_browser.Dispose();
				          	})
				.Wait(TimeSpan.FromSeconds(2));
			if (!success)
			{
				_closeByWatiNCloseNDisposeFailed = true;
				return TryResult.Failure;
			}
			return TryResult.Passed;
		}




		private static SystemLevelLock MakeBrowserLock() { return new SystemLevelLock("TestBrowserLock"); }

		private static void LockBrowser() { BrowserLock = MakeBrowserLock(); }

		private static void ReleaseBrowser()
		{
			BrowserLock.Dispose();
			BrowserLock = null;
		}

	}
}