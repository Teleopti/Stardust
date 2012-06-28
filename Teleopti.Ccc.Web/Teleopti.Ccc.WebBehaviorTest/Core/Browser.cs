using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class Browser
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (Browser));

		private static bool CloseByWatiNCloseNDisposeFailed = false;

		public static IE Current
		{
			get
			{
				if (!IsStarted())
					Start();
				return ScenarioBrowser;
			}
		}

		
		private static IE ScenarioBrowser { get { return ScenarioContext.Current.Value<IE>(); } set { ScenarioContext.Current.Value(value); } }
		private static IDisposable BrowserLock { get { return ScenarioContext.Current.Value<SystemLevelLock>(); } set { ScenarioContext.Current.Value((SystemLevelLock) value); } }

		private static void LockBrowser() { BrowserLock = MakeBrowserLock(); }

		private static void ReleaseBrowser()
		{
			BrowserLock.Dispose();
			BrowserLock = null;
		}

		private static SystemLevelLock MakeBrowserLock() { return new SystemLevelLock("WatiNBrowserLock"); }

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private static void Start()
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
			ScenarioBrowser = browser;
		}

		public static bool IsStarted() { return ScenarioBrowser != null; }

		public static void MakeSureBrowserIsNotRunning()
		{
			using (MakeBrowserLock())
			{
				var startTime = DateTime.Now;
				Log.Write("Making sure browser is not running");
				var result = AttemptToCloseBrowser(new Func<bool>[]
				                                   	{
				                                   		CloseByClosingMainWindow,
				                                   		CloseByKillingProcesses
				                                   	});
				if (!result)
					throw new ApplicationException("Browser failed to close when making sure it isnt running");
				var methodTime = DateTime.Now.Subtract(startTime);
				Log.Write("Making sure browser was not running took " + methodTime);
			}
		}

		public static void GentlyClose()
		{
			try
			{
				Log.Write("Closing the browser");
				var browser = ScenarioBrowser;
				ScenarioBrowser = null;
				var result = AttemptToCloseBrowser(new Func<bool>[]
				                                   	{
				                                   		() => CloseByWatiNCloseNDispose(browser),
				                                   		() => CloseByWatiNForceClose(browser)
				                                   	});
				if (!result)
					throw new ApplicationException("Browser failed to close.");
			}
			finally
			{
				ReleaseBrowser();
			}
		}

		public static void ForciblyClose()
		{
			try
			{
				Log.Write("Closing the browser");
				var browser = ScenarioBrowser;
				ScenarioBrowser = null;
				var result = AttemptToCloseBrowser(new Func<bool>[]
				                                   	{
				                                   		() => CloseByWatiNCloseNDispose(browser),
				                                   		CloseByClosingMainWindow,
				                                   		() => CloseByWatiNForceClose(browser),
				                                   		CloseByKillingProcesses
				                                   	});
				if (!result)
					throw new ApplicationException("Browser failed to close.");
			}
			finally
			{
				ReleaseBrowser();
			}
		}

		private static bool AttemptToCloseBrowser(IEnumerable<Func<bool>> browserClosingActions)
		{
			return browserClosingActions.Any(AttemptToCloseBrowser);
		}

		private static bool AttemptToCloseBrowser(Func<bool> browserClosingAction)
		{
			bool successfulAttempt;
			try
			{
				successfulAttempt = browserClosingAction.Invoke();
			}
			catch (Exception exception)
			{
				Log.Write("Exception occurred trying to close browser: " + exception);
				Log.Write("Trying something else...");
				return false;
			}
			if (successfulAttempt)
			{
				var browserWasClosed = WaitForBrowserToClose();
				if (browserWasClosed)
					Log.Write("Successfully closed the browser");
				else
				{
					Log.Write("Browser is still runing");
					Log.Write("Trying something else...");
				}
				return browserWasClosed;
			}
			return false;
		}

		private static bool WaitForBrowserToClose()
		{
			Log.Write("Waiting for browser to close");
			Func<bool> browserIsStopped = () => !Process.GetProcessesByName(@"iexplore").Any();
			return browserIsStopped.WaitUntil(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(10));
		}

		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		private static bool CloseByWatiNCloseNDispose(WatiN.Core.Browser browser)
		{
			if (CloseByWatiNCloseNDisposeFailed)
				return false;
			Log.Write("Trying to close browser by WatiN Close() and Dispose()");
			var success = Task.Factory.StartNew(() =>
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

		private static bool CloseByWatiNForceClose(IE browser)
		{
			Log.Write("Trying to close browser by WatiN ForceClose()");
			browser.ForceClose();
			return true;
		}

		private static bool CloseByClosingMainWindow()
		{
			Log.Write("Trying to close browser by closing its main window(s)");
			var ieProcesses = Process.GetProcessesByName(@"iexplore");
			foreach (var process in ieProcesses)
			{
				process.CloseMainWindow();
			}
			return true;
		}

		private static bool CloseByKillingProcesses()
		{
			Log.Write("Trying to close browser by killing its process(es)");
			var ieProcesses = Process.GetProcessesByName(@"iexplore");
			foreach (var process in ieProcesses)
			{
				process.Kill();
			}
			return true;
		}
	}
}
