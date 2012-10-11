using System;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class WatiNSingleBrowserFireFoxHandler : IBrowserHandler<FireFox>
	{
		private const string ProcessName = "firefox";

		private static readonly ILog Log = LogManager.GetLogger(typeof (WatiNSingleBrowserFireFoxHandler));

		private static IDisposable BrowserLock { get { return ScenarioContext.Current.Value<SystemLevelLock>(); } set { ScenarioContext.Current.Value((SystemLevelLock) value); } }

		private FireFox _browser;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public FireFox Start()
		{
			LockBrowser();
			Settings.AutoCloseDialogs = true;
			Settings.AutoMoveMousePointerToTopLeft = false;
			Settings.HighLightColor = "Green";
			Settings.HighLightElement = true;
			_browser = new FireFox();
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

		public FireFox Restart() { throw new NotImplementedException(); }


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



		private TryResult TryCloseByWatiNCloseNDispose()
		{
			var result = Task.Factory
				.StartNew(() =>
				          	{
				          		_browser.Close();
								_browser.Dispose();
				          	})
				.Wait(TimeSpan.FromSeconds(2));
			return result ? TryResult.Passed : TryResult.Failure;
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