using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl
{
	public class IEHandler : IBrowserHandler<IE>
	{
		private const string ProcessName = "iexplore";

		private static readonly ILog Log = LogManager.GetLogger(typeof(IEHandler));

		private static IDisposable BrowserLock { get { return ScenarioContext.Current.Value<SystemLevelLock>(); } set { ScenarioContext.Current.Value((SystemLevelLock)value); } }

		private static bool CloseByWatiNCloseNDisposeFailed = false;

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
			var browser = new IE();
			browser.ClearCache();
			browser.ClearCookies();
			browser.BringToFront();
			return browser;
		}

		public void PrepareForTestRun() { MakeSureBrowserIsNotRunning(); }

		public void Close(IE browser)
		{
			var startTime = DateTime.Now;

			try
			{
				var result = BrowserProcessHelpers.AttemptToCloseProcess(
					ProcessName,
					new Func<bool>[]
						{
							() => CloseByWatiNCloseNDispose(browser),
							() => BrowserProcessHelpers.CloseByClosingMainWindow(ProcessName),
							() => CloseByWatiNForceClose(browser),
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
			if (CloseByWatiNCloseNDisposeFailed)
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
				CloseByWatiNCloseNDisposeFailed = true;
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