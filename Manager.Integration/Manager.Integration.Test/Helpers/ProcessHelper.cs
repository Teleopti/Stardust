using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using Manager.Integration.Test.Properties;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;

namespace Manager.Integration.Test.Helpers
{
	public static class ProcessHelper
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (ProcessHelper));


#if DEBUG
		private static readonly string _buildMode = "Debug";
#else
        private static string _buildMode = "Release";
#endif


		private static ManualResetEventSlim _closeProcessManualResetEventSlim;

		public static void ShutDownAllManagerIntegrationConsoleHostProcesses()
		{
			Logger.DebugWithLineNumber("Started.");

			var consoleHostname = new FileInfo(Settings.Default.ManagerIntegrationConsoleHostAssemblyName);

			var fileNameWithNoExtension = consoleHostname.Name.Replace(consoleHostname.Extension,
			                                                           string.Empty);

			ShutDownAllProcesses(fileNameWithNoExtension);

			Logger.DebugWithLineNumber("Finished.");
		}

		public static void ShowAllManagerIntegrationConsoleHostProcesses()
		{
			var consoleHostname = new FileInfo(Settings.Default.ManagerIntegrationConsoleHostAssemblyName);

			var fileNameWithNoExtension = consoleHostname.Name.Replace(consoleHostname.Extension,
			                                                           string.Empty);

			ShowAllProcesses(fileNameWithNoExtension);
		}


		public static void CloseProcess(Process process)
		{
			if (process != null)
			{
				try
				{
					_closeProcessManualResetEventSlim = new ManualResetEventSlim();

					process.EnableRaisingEvents = true;
					process.Exited += ProcessOnExited;

					process.CloseMainWindow();

					_closeProcessManualResetEventSlim.Wait();
				}

				catch (Exception)
				{
				}

				finally
				{
					process.Exited -= ProcessOnExited;
				}
			}
		}

		private static void ProcessOnExited(object sender,
		                                    System.EventArgs eventArgs)
		{
			_closeProcessManualResetEventSlim.Set();
		}

		public static Process StartManagerIntegrationConsoleHostProcess(int numberOfNodesToStart)
		{
			var managerIntegrationConsoleHostLocation =
				new DirectoryInfo(Settings.Default.ManagerIntegrationConsoleHostLocation + _buildMode);

			var managerIntegrationConsoleHostAssemblyName =
				Settings.Default.ManagerIntegrationConsoleHostAssemblyName;

			return StartProcess(managerIntegrationConsoleHostLocation,
			                    managerIntegrationConsoleHostAssemblyName,
			                    numberOfNodesToStart);
		}

		public static Process StartProcess(DirectoryInfo processDirectory,
		                                   string processFileName,
		                                   int numberOfNodesToStart)
		{
			var process = CreateProcess(processDirectory,
			                            processFileName,
			                            numberOfNodesToStart);

			process.Start();

			return process;
		}

		public static Process CreateProcess(DirectoryInfo processDirectory,
		                                    string processFileName,
		                                    int numberOfNodesToStart)
		{
			var processStartInfo = new ProcessStartInfo
			{
				CreateNoWindow = false,
				UseShellExecute = false,
				Arguments = numberOfNodesToStart.ToString(),
				WindowStyle = ProcessWindowStyle.Normal,
				WorkingDirectory = processDirectory.FullName,
				RedirectStandardOutput = false,
				FileName = Path.Combine(processDirectory.FullName,
				                        processFileName)
			};

			var processToReturn = new Process
			{
				StartInfo = processStartInfo
			};

			return processToReturn;
		}

		public static void ShutDownAllProcesses(string processName)
		{
			Logger.DebugWithLineNumber("Started.");

			var processes = Process.GetProcessesByName(processName);

			if (processes.Any())
			{
				foreach (var process in processes)
				{
					var name = process.ProcessName;
					var fileName = process.StartInfo.FileName;
					var userName = process.StartInfo.UserName;

					Logger.DebugWithLineNumber(
						string.Format("Started : Process name : {0}, Process path : {1}, Process started by : {2}",
						              name,
						              fileName,
						              userName));

					CloseProcess(process);

					Logger.DebugWithLineNumber(
						string.Format("Finished : Process name : {0}, Process path : {1}, Process started by : {2}",
						              name,
						              fileName,
						              userName));
				}
			}

			Logger.DebugWithLineNumber("Finished.");
		}


		public static void ShowAllProcesses(string processName)
		{
			var processes = Process.GetProcessesByName(processName);

			if (processes.Any())
			{
				foreach (var process in processes)
				{
					Logger.DebugWithLineNumber(string.Format("Process name : {0}, Process path : {1}, Process started by : {2}",
					                                               process.ProcessName,
					                                               process.StartInfo.FileName,
					                                               process.StartInfo.UserName));
				}
			}
		}

		public static int NumberOfProcesses(string processName)
		{
			var processes = Process.GetProcessesByName(processName);

			return processes.Length;
		}
	}
}