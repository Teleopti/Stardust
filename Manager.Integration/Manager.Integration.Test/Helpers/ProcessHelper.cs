using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Manager.Integration.Test.Properties;

namespace Manager.Integration.Test.Helpers
{
    public static class ProcessHelper
    {
#if DEBUG
        private static string _buildMode = "Debug";
#else
        private static string _buildMode = "Release";
#endif

        public static void ShutDownAllManagerIntegrationConsoleHostProcesses()
        {
            var consoleHostname = new FileInfo(Settings.Default.ManagerIntegrationConsoleHostAssemblyName);

            var fileNameWithNoExtension = consoleHostname.Name.Replace(consoleHostname.Extension,
                                                                       string.Empty);

            ShutDownAllProcesses(fileNameWithNoExtension);
        }

        public static void CloseProcess(Process process)
        {
            if (process != null)
            {
                try
                {
                    process.CloseMainWindow();
                    process.WaitForExit();
                }
                catch (Exception)
                {
                }
            }
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
            ProcessStartInfo processStartInfo = new ProcessStartInfo
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
            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Any())
            {
                foreach (var process in processes)
                {
                    process.CloseMainWindow();
                    process.WaitForExit();
                }
            }
        }
    }
}