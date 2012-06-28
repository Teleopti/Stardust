using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl
{
	public static class BrowserProcessHelpers
	{
		public static bool CloseByClosingMainWindow(string processName)
		{
			var processes = Process.GetProcessesByName(processName);
			foreach (var process in processes)
				process.CloseMainWindow();
			return true;
		}

		public static bool CloseByKillingProcesses(string processName)
		{
			var processes = Process.GetProcessesByName(processName);
			foreach (var process in processes)
				process.Kill();
			return true;
		}

		public static bool WaitForProcessToExit(string processName)
		{
			Func<bool> isStopped = () => !Process.GetProcessesByName(processName).Any();
			return isStopped.WaitUntil(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(10));
		}



		public static bool AttemptToCloseProcess(string processName, IEnumerable<Func<bool>> processClosingActions)
		{
			return processClosingActions.Any(action => AttemptToCloseProcess(processName, action));
		}

		private static bool AttemptToCloseProcess(string processName, Func<bool> processClosingAction)
		{
			var successfulAttempt = false;
			try
			{
				successfulAttempt = processClosingAction.Invoke();
			}
			catch (Exception)
			{
				return false;
			}
			if (successfulAttempt)
			{
				return WaitForProcessToExit(processName);
			}
			return false;
		}

	}
}