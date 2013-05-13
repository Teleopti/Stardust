using System;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl
{
	public class WatiNSingleBrowserFireFoxActivator : IBrowserActivator<FireFox>
	{
		private const string ProcessName = "firefox";

		private static IDisposable BrowserLock { get { return ScenarioContext.Current.Value<SystemLevelLock>(); } set { ScenarioContext.Current.Value((SystemLevelLock) value); } }

		public FireFox Internal { get; set; }

		public void Start()
		{
			LockBrowser();
			Settings.AutoCloseDialogs = true;
			Settings.AutoMoveMousePointerToTopLeft = false;
			Settings.HighLightColor = "Green";
			Settings.HighLightElement = true;
			Internal = new FireFox();
			Internal.BringToFront();
		}

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
				Internal = null;
				ReleaseBrowser();
			}
		}

		public void NotifyBeforeTestRun() { MakeSureBrowserIsNotRunning(); }

		public void NotifyBeforeScenario() { }

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
								Internal.Close();
								Internal.Dispose();
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