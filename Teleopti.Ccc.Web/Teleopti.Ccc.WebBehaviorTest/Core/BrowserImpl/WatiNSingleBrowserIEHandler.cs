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
			_browser = new IE();
			_browser.ClearCache();
			_browser.ClearCookies();
			_browser.BringToFront();
			return _browser;
		}

		public void PrepareForTestRun() { MakeSureBrowserIsNotRunning(); }

		public void Close()
		{
			var startTime = DateTime.Now;

			try
			{
				var result = BrowserProcessHelpers.AttemptToCloseProcess(
					ProcessName,
					new Func<bool>[]
						{
							() => CloseByWatiNCloseNDispose(_browser),
							() => BrowserProcessHelpers.CloseByClosingMainWindow(ProcessName),
							() => CloseByWatiNForceClose(_browser),
							() => BrowserProcessHelpers.CloseByKillingProcesses(ProcessName)
						});
				if (!result)
					throw new ApplicationException("Browser failed to close.");
			}
			finally
			{
				ReleaseBrowser();
			}

			Log.Write("Close took " + DateTime.Now.Subtract(startTime));
		}





		private void MakeSureBrowserIsNotRunning()
		{
			var startTime = DateTime.Now;

			using (MakeBrowserLock())
			{
				var result = BrowserProcessHelpers.AttemptToCloseProcess(
					ProcessName,
					new Func<bool>[]
						{
							() => BrowserProcessHelpers.CloseByClosingMainWindow(ProcessName),
							() => BrowserProcessHelpers.CloseByKillingProcesses(ProcessName)
						});
				if (!result)
					throw new ApplicationException("Browser failed to close when making sure it isnt running");
			}

			Log.Write("MakeSureBrowserIsNotRunning " + DateTime.Now.Subtract(startTime));
		}



		private static bool CloseByWatiNForceClose(IE browser)
		{
			browser.ForceClose();
			return true;
		}

		private static bool CloseByWatiNCloseNDispose(WatiN.Core.Browser browser)
		{
			if (_closeByWatiNCloseNDisposeFailed)
				return false;
			var success = Task.Factory
				.StartNew(() =>
				          	{
				          		browser.Close();
				          		browser.Dispose();
				          	})
				.Wait(TimeSpan.FromSeconds(2));
			if (!success)
			{
				_closeByWatiNCloseNDisposeFailed = true;
				return false;
			}
			return true;
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