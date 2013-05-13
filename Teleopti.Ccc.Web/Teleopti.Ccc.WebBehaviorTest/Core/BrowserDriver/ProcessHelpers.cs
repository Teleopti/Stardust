using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class ProcessNotFoundException : Exception
	{
		public ProcessNotFoundException(string message)
			: base(message)
		{
		}
	}

	public enum TryResult
	{
		Failure,
		Passed,
		NotFound
	}

	public static class ProcessHelpers
	{
		internal static void CloseByClosingMainWindow(string processName, IntPtr windowHandle)
		{
			ProcessByMainWindow(processName, windowHandle).CloseMainWindow();
		}

		internal static TryResult TryCloseByClosingMainWindow(string processName, IntPtr windowHandle)
		{
			return Try(() => CloseByClosingMainWindow(processName, windowHandle));
		}

		internal static void CloseByClosingMainWindow(string processName, int processId)
		{
			ProcessById(processName, processId).CloseMainWindow();
		}

		internal static TryResult TryCloseByClosingMainWindow(string processName, int processId)
		{
			return Try(() => CloseByClosingMainWindow(processName, processId));
		}

		internal static void CloseByClosingMainWindow(string processName)
		{
			ProcessByName(processName).ForEach(p => p.CloseMainWindow());
		}

		internal static TryResult TryCloseByClosingMainWindow(string processName)
		{
			return Try(() => CloseByClosingMainWindow(processName));
		}



		internal static void CloseByKillingProcess(string processName, IntPtr windowHandle)
		{
			ProcessByMainWindow(processName, windowHandle).Kill();
		}

		internal static TryResult TryCloseByKillingProcess(string processName, IntPtr windowHandle)
		{
			return Try(() => CloseByKillingProcess(processName, windowHandle));
		}

		internal static void CloseByKillingProcess(string processName, int processId)
		{
			ProcessById(processName, processId).Kill();
		}

		internal static TryResult TryCloseByKillingProcess(string processName, int processId)
		{
			return Try(() => CloseByKillingProcess(processName, processId));
		}

		internal static void CloseByKillingProcess(string processName)
		{
			ProcessByName(processName).ForEach(p => p.Kill());
		}

		internal static TryResult TryCloseByKillingProcess(string processName)
		{
			return Try(() => CloseByKillingProcess(processName));
		}




		internal static int ProcessIdForMainWindow(string processName, IntPtr windowHandle)
		{
			return ProcessByMainWindow(processName, windowHandle).Id;
		}





		internal static bool TryToCloseProcess(string processName, params Func<TryResult>[] attempts)
		{
			Func<bool> isClosed = () => QueryProcessesByName(processName).Count() == 0;
			if (isClosed.Invoke())
				return true;
			return attempts.Any(attempt => TryToCloseProcess(isClosed, attempt));
		}

		internal static bool TryToCloseProcess(string processName, IntPtr windowHandle, params Func<TryResult>[] attempts)
		{
			Func<bool> isClosed = () => QueryProcessesByMainWindow(processName, windowHandle).Count() == 0;
			if (isClosed.Invoke())
				return true;
			return attempts.Any(attempt => TryToCloseProcess(isClosed, attempt));
		}

		internal static bool TryToCloseProcess(string processName, int processId, params Func<TryResult>[] attempts)
		{
			Func<bool> isClosed = () => QueryProcessesById(processName, processId).Count() == 0;
			if (isClosed.Invoke())
				return true;
			return attempts.Any(attempt => TryToCloseProcess(isClosed, attempt));
		}

		private static bool TryToCloseProcess(Func<bool> isClosed, Func<TryResult> attempt)
		{
			TryResult result;
			try
			{
				result = attempt.Invoke();
			}
			catch (Exception)
			{
				return false;
			}
			if (result == TryResult.Passed)
				return isClosed.WaitUntil(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(5));
			if (result == TryResult.NotFound)
				return isClosed.Invoke();
			return false;
		}




		private static Process ProcessByMainWindow(string processName, IntPtr windowHandle)
		{
			var processes = QueryProcessesByMainWindow(processName, windowHandle);
			if (processes.Count() == 1)
				return processes.Single();
			throw new ProcessNotFoundException("Single process " + processName + " with window handle " + windowHandle + " not found.");
		}

		private static Process ProcessById(string processName, int processId)
		{
			var processes = QueryProcessesById(processName, processId);
			if (processes.Count() == 1)
				return processes.Single();
			throw new ProcessNotFoundException("Single process " + processName + " with id " + processId + " not found.");
		}

		private static IEnumerable<Process> ProcessByName(string processName)
		{
			var processes = QueryProcessesByName(processName);
			if (processes.Any())
				return processes;
			throw new ProcessNotFoundException("Single process " + processName + " not found.");
		}




		private static IEnumerable<Process> QueryProcessesByMainWindow(string processName, IntPtr windowHandle)
		{
			var processes = Process.GetProcessesByName(processName);
			return (from p in processes where p.MainWindowHandle == windowHandle select p).ToArray();
		}

		private static IEnumerable<Process> QueryProcessesById(string processName, int processId)
		{
			var processes = Process.GetProcessesByName(processName);
			return (from p in processes where p.Id == processId select p).ToArray();
		}

		private static IEnumerable<Process> QueryProcessesByName(string processName)
		{
			return Process.GetProcessesByName(processName);
		}




		private static TryResult Try(Action action)
		{
			try
			{
				action.Invoke();
				return TryResult.Passed;
			}
			catch (ProcessNotFoundException)
			{
				return TryResult.NotFound;
			}
			catch (Exception)
			{
				return TryResult.Failure;
			}
		}


	}
}