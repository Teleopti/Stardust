using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl
{
	public static class BrowserProcessHelpers
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		internal static bool CloseByClosingMainWindow(string processName, IntPtr windowHandle)
		{
			var processes = Process.GetProcessesByName(processName);
			var processesForWindowHandle = from p in processes where p.MainWindowHandle == windowHandle select p;
			processesForWindowHandle.ForEach(p => p.CloseMainWindow());
			return true;
		}

		internal static bool CloseByKillingProcess(string processName, int processId)
		{
			var processes = Process.GetProcessesByName(processName);
			var processesForWindowHandle = from p in processes where p.Id == processId select p;
			processesForWindowHandle.ForEach(p => p.Kill());
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		internal static int ProcessIdForMainWindow(string processName, IntPtr windowHandle)
		{
			var processes = Process.GetProcessesByName(processName);
			return (from p in processes where p.MainWindowHandle == windowHandle select p.Id).Single();
		}

		internal static bool CloseByClosingMainWindow(string processName)
		{
			var processes = Process.GetProcessesByName(processName);
			foreach (var process in processes)
				process.CloseMainWindow();
			return true;
		}

		internal static bool CloseByKillingProcesses(string processName)
		{
			var processes = Process.GetProcessesByName(processName);
			foreach (var process in processes)
				process.Kill();
			return true;
		}



		internal static bool AttemptToCloseProcess(string processName, IEnumerable<Func<bool>> processClosingActions)
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
				return WaitForProcessToExit(processName);
			return false;
		}



		internal static bool WaitForProcessToExit(string processName)
		{
			Func<bool> isStopped = () => !Process.GetProcessesByName(processName).Any();
			return isStopped.WaitUntil(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(10));
		}


	}
}