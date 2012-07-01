using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl
{
	public static class BrowserProcessHelpers
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		internal static bool CloseByClosingMainWindow(string processName, IntPtr windowHandle)
		{
			var processes = Process.GetProcessesByName(processName);
			foreach (var process in processes)
			{
				if (process.MainWindowHandle == windowHandle)
				{
					process.CloseMainWindow();
					return true;
				}
			}
			return false;
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

		internal static bool WaitForProcessToExit(string processName)
		{
			Func<bool> isStopped = () => !Process.GetProcessesByName(processName).Any();
			return isStopped.WaitUntil(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(10));
		}



		internal static bool AttemptToCloseProcess(string processName, IEnumerable<Func<bool>> processClosingActions)
		{
			System.IO.File.AppendAllText(@"C:\AfterTestRun.txt", "AttemptToCloseProcess\r\n");
			return processClosingActions.Any(action =>
			                                 	{
													System.IO.File.AppendAllText(@"C:\AfterTestRun.txt", "AttemptToCloseProcess/Action\r\n");
													return AttemptToCloseProcess(processName, action);
			                                 	});
		}

		private static bool AttemptToCloseProcess(string processName, Func<bool> processClosingAction)
		{
			System.IO.File.AppendAllText(@"C:\AfterTestRun.txt", "AttemptToCloseProcess\r\n");
			var successfulAttempt = false;
			try
			{
				System.IO.File.AppendAllText(@"C:\AfterTestRun.txt", "AttemptToCloseProcess/Invoke\r\n");
				successfulAttempt = processClosingAction.Invoke();
				System.IO.File.AppendAllText(@"C:\AfterTestRun.txt", "/AttemptToCloseProcess/Invoke" + successfulAttempt + "\r\n");
			}
			catch (Exception e)
			{
				System.IO.File.AppendAllText(@"C:\AfterTestRun.txt", "AttemptToCloseProcess/Exception" + e + "\r\n");
				return false;
			}
			if (successfulAttempt)
			{
				System.IO.File.AppendAllText(@"C:\AfterTestRun.txt", "AttemptToCloseProcess/Wait\r\n");
				return WaitForProcessToExit(processName);
			}
			return false;
		}

	}
}