using System;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl
{
	public class FireFoxHandler : IBrowserHandler<FireFox>
	{
		private const string ProcessName = "firefox";

		private static readonly ILog Log = LogManager.GetLogger(typeof (FireFoxHandler));

		private static IDisposable BrowserLock { get { return ScenarioContext.Current.Value<SystemLevelLock>(); } set { ScenarioContext.Current.Value((SystemLevelLock) value); } }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public FireFox Start()
		{
			LockBrowser();
			Settings.AutoCloseDialogs = true;
			Settings.AutoMoveMousePointerToTopLeft = false;
			Settings.HighLightColor = "Green";
			Settings.HighLightElement = true;
			var browser = new FireFox();
			browser.BringToFront();
			return browser;
		}

		public void PrepareForTestRun() { MakeSureBrowserIsNotRunning(); }

		public void Close(FireFox browser)
		{
			var startTime = DateTime.Now;

			try
			{
				var result = BrowserProcessHelpers.AttemptToCloseProcess(
					ProcessName,
					new Func<bool>[]
						{
							() => CloseByWatiNCloseNDispose(browser as FireFox),
							() => BrowserProcessHelpers.CloseByClosingMainWindow(ProcessName),
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



		private static bool CloseByWatiNCloseNDispose(WatiN.Core.Browser browser)
		{
			var success = Task.Factory
				.StartNew(() =>
				          	{
				          		browser.Close();
				          		browser.Dispose();
				          	})
				.Wait(TimeSpan.FromSeconds(2));
			if (!success)
			{
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