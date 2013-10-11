using System;
using System.Diagnostics;
using System.Linq;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver.WatiNIE
{
	public class WatiNParallelBrowserIEActivator : IBrowserActivator
	{
		private const string ProcessName = "iexplore";

		public IE Internal { get; set; }

		public void SetTimeout(TimeSpan timeout)
		{
			Settings.WaitForCompleteTimeOut = Convert.ToInt32(timeout.TotalSeconds);
			Settings.WaitUntilExistsTimeOut = Convert.ToInt32(timeout.TotalSeconds);
		}

		public void Start(TimeSpan timeout, TimeSpan retry)
		{
			Settings.AutoCloseDialogs = true;
			Settings.AutoMoveMousePointerToTopLeft = false;
			Settings.HighLightColor = "Green";
			Settings.HighLightElement = true;
			Settings.MakeNewIe8InstanceNoMerge = true;
			Settings.MakeNewIeInstanceVisible = true;
			SetTimeout(timeout);

			Internal = new IE { AutoClose = true };
			Internal.ClearCache();
			Internal.ClearCookies();
			Internal.BringToFront();
		}

		public void Close()
		{
			if (Internal == null)
				return;
			Internal.Close();
			Internal.Dispose();
			Internal = null;
		}

		public void NotifyBeforeTestRun()
		{
			closeAllBrowsersIfIAmTheOnlyTestRun();
		}

		private static void closeAllBrowsersIfIAmTheOnlyTestRun()
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

		public void NotifyBeforeScenario() { }

		public IBrowserInteractions GetInteractions()
		{
			throw new NotImplementedException();
		}
	}
}