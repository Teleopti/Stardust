using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using Manager.Integration.Test.Properties;

namespace Manager.Integration.Test.Helpers
{
    public static class ProcessHelper
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (ProcessHelper));

#if DEBUG
        private static string _buildMode = "Debug";
#else
        private static string _buildMode = "Release";
#endif

        public static void ShutDownAllManagerIntegrationConsoleHostProcesses()
        {
            Logger.Info("ProcessHelper.ShutDownAllManagerIntegrationConsoleHostProcesses :  Started.");

            var consoleHostname = new FileInfo(Settings.Default.ManagerIntegrationConsoleHostAssemblyName);

            var fileNameWithNoExtension = consoleHostname.Name.Replace(consoleHostname.Extension,
                                                                       string.Empty);
            ShutDownAllProcesses(fileNameWithNoExtension);

            Logger.Info("ProcessHelper.ShutDownAllManagerIntegrationConsoleHostProcesses :  Finished.");
        }

        public static void ShowAllManagerIntegrationConsoleHostProcesses()
        {
            var consoleHostname = new FileInfo(Settings.Default.ManagerIntegrationConsoleHostAssemblyName);

            var fileNameWithNoExtension = consoleHostname.Name.Replace(consoleHostname.Extension,
                                                                       string.Empty);

            ShowAllProcesses(fileNameWithNoExtension);
        }

        private static ManualResetEventSlim CloseProcessManualResetEventSlim;

        public static void CloseProcess(Process process)
        {
            if (process != null)
            {
                try
                {
                    CloseProcessManualResetEventSlim = new ManualResetEventSlim();

                    process.Exited += ProcessOnExited;

                    process.CloseMainWindow();

                    CloseProcessManualResetEventSlim.Wait();

                    process.Exited -= ProcessOnExited;
                }
                catch (Exception)
                {
                }
            }
        }

        private static void ProcessOnExited(object sender,
                                            System.EventArgs eventArgs)
        {
            CloseProcessManualResetEventSlim.Set();
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

        public static void ShowAllProcesses(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Any())
            {
                foreach (var process in processes)
                {
                    Logger.Info(string.Format("Process name : {0}, Process path : {1}, Process started by : {2}",
                                              process.ProcessName,
                                              process.StartInfo.FileName,
                                              process.StartInfo.UserName));
                }
            }
        }

        public static void ShutDownAllProcesses(string processName)
        {
            Logger.Info("ProcessHelper.ShutDownAllProcesses method started.");

            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Any())
            {
                foreach (var process in processes)
                {
                    var name = process.ProcessName;
                    var fileName = process.StartInfo.FileName;
                    var userName = process.StartInfo.UserName;

                    Logger.Info(string.Format("ShutDownAllProcesses... Started : Process name : {0}, Process path : {1}, Process started by : {2}",
                                              name,
                                              fileName,
                                              userName));

                    CloseProcess(process);

                    Logger.Info(string.Format("ShutDownAllProcesses... Finished : Process name : {0}, Process path : {1}, Process started by : {2}",
                                              name,
                                              fileName,
                                              userName));
                }
            }

            Logger.Info("ProcessHelper.ShutDownAllProcesses method finished.");
        }
    }
}