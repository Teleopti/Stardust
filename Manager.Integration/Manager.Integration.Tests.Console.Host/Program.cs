using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Manager.IntegrationTest.Console.Host.Properties;

namespace Manager.IntegrationTest.Console.Host
{
    public static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (Program));

        private const string CopiedManagerConfigName = "Manager.config";

#if (DEBUG)
        private static string _buildMode = "Debug";
#else
        private static string _buildMode = "Release";
#endif

        public static FileInfo ManagerConfigurationFile { get; set; }

        public static FileInfo CopiedManagerConfigurationFile { get; set; }

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine handler,
                                                        bool add);

        // A delegate type to be used as the handler routine 
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes ctrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            CtrlCEvent = 0,
            CtrlBreakEvent,
            CtrlCloseEvent,
            CtrlLogoffEvent = 5,
            CtrlShutdownEvent
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            if (ctrlType == CtrlTypes.CtrlCloseEvent ||
                ctrlType == CtrlTypes.CtrlShutdownEvent)
            {
                foreach (var appDomain in AppDomains.Values)
                {
                    AppDomain.Unload(appDomain);
                }

                return true;
            }

            return false;
        }

        private static string AddEndingSlash(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (!path.EndsWith(@"\"))
                {
                    path += @"\";
                }
            }

            return path;
        }

        public static ConcurrentDictionary<string, AppDomain> AppDomains { get; set; }

        public static void AddOrUpdateAppDomains(string key,
                                                 AppDomain value)
        {
            AppDomain appdomain = AppDomains.AddOrUpdate(key,
                                                         value,
                                                         (name,
                                                          val) => value);
        }

        private static void Main(string[] args)
        {
            SetConsoleCtrlHandler(ConsoleCtrlCheck,
                                  true);

            System.Console.CancelKeyPress += Console_CancelKeyPress;

            AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;

            XmlConfigurator.Configure();

            AppDomains = new ConcurrentDictionary<string, AppDomain>();

            Evidence adevidence = AppDomain.CurrentDomain.Evidence;

            Logger.Info("Manager.IntegrationTest.Console.Host.Main started.");

            var directoryManagerConfigurationFileFullPath =
                new DirectoryInfo(AddEndingSlash(Settings.Default.ManagerConfigurationFileFullPath + _buildMode));

            ManagerConfigurationFile =
                new FileInfo(directoryManagerConfigurationFileFullPath.FullName +
                             Settings.Default.ManagerConfigurationFileName);

            CopiedManagerConfigurationFile =
                CopyManagerConfigurationFile(ManagerConfigurationFile,
                                             CopiedManagerConfigName);

            var tasks = new List<Task>();

            var managerTask = new Task(() =>
            {
                var managerAssemblyLocationFullPath = Settings.Default.ManagerAssemblyLocationFullPath;

                var directoryManagerAssemblyLocationFullPath =
                    new DirectoryInfo(AddEndingSlash(managerAssemblyLocationFullPath + _buildMode));

                // Start manager.
                var managerAppDomainSetup = new AppDomainSetup
                {
                    ApplicationBase = directoryManagerAssemblyLocationFullPath.FullName,
                    ApplicationName = Settings.Default.ManagerAssemblyName,
                    ShadowCopyFiles = "true",
                    ConfigurationFile = CopiedManagerConfigurationFile.FullName
                };

                AppDomain managerAppDomain = AppDomain.CreateDomain(managerAppDomainSetup.ApplicationName,
                                                                    adevidence,
                                                                    managerAppDomainSetup);

                AddOrUpdateAppDomains("Manager",
                                      managerAppDomain);

                managerAppDomain.ExecuteAssembly(managerAppDomainSetup.ApplicationBase +
                                                 managerAppDomainSetup.ApplicationName);
            });

            tasks.Add(managerTask);

            var directoryNodeConfigurationFileFullPath =
                new DirectoryInfo(AddEndingSlash(Settings.Default.NodeConfigurationFileFullPath + _buildMode));

            var nodeConfigurationFile =
                new FileInfo(directoryNodeConfigurationFileFullPath.FullName +
                             Settings.Default.NodeConfigurationFileName);

            var nodeconfigurationFiles = new Dictionary<string, FileInfo>();

            var portStartNumber =
                Settings.Default.NodeEndpointPortNumberStart;

            var numberOfNodesToStart = Settings.Default.NumberOfNodesToStart;

            if (args.Any())
            {
                numberOfNodesToStart = Convert.ToInt32(args[0]);
            }

            if (numberOfNodesToStart > 0)
            {
                for (var i = 1; i <= numberOfNodesToStart; i++)
                {
                    var nodeName = "Node" + i;

                    var configName = nodeName + ".config";

                    var portNumber = portStartNumber + (i - 1);

                    var endPointUri =
                        new Uri(Settings.Default.NodeEndpointUriTemplate.Replace("PORTNUMBER",
                                                                                 portNumber.ToString()));

                    var copiedConfigurationFile =
                        CreateNodeConfigFile(nodeConfigurationFile,
                                             configName,
                                             nodeName,
                                             new Uri(Settings.Default.ManagerLocationUri),
                                             endPointUri,
                                             Settings.Default.HandlerAssembly);

                    nodeconfigurationFiles.Add(nodeName,
                                               copiedConfigurationFile);
                }
            }


            var directoryNodeAssemblyLocationFullPath =
                new DirectoryInfo(AddEndingSlash(Settings.Default.NodeAssemblyLocationFullPath + _buildMode));

            foreach (var nodeconfigurationFile in nodeconfigurationFiles)
            {
                var nodeTask = new Task(() =>
                {
                    var nodeAppDomainSetup = new AppDomainSetup
                    {
                        ApplicationBase = directoryNodeAssemblyLocationFullPath.FullName,
                        ApplicationName = Settings.Default.NodeAssemblyName,
                        ShadowCopyFiles = "true",
                        ConfigurationFile = nodeconfigurationFile.Value.FullName,
                    };

                    var nodeAppDomain = AppDomain.CreateDomain(nodeconfigurationFile.Key,
                                                               adevidence,
                                                               nodeAppDomainSetup);

                    AddOrUpdateAppDomains(nodeconfigurationFile.Value.Name,
                                          nodeAppDomain);

                    var assemblyToExecute =
                        nodeAppDomainSetup.ApplicationBase + nodeAppDomainSetup.ApplicationName;

                    nodeAppDomain.ExecuteAssembly(assemblyToExecute);
                });

                tasks.Add(nodeTask);
            }

            foreach (var task in tasks)
            {
                task.Start();
            }

            QuitEvent.WaitOne();

            Logger.Info("Manager.IntegrationTest.Console.Host.Main unload app domains.");

            foreach (var appDomain in AppDomains.Values)
            {
                AppDomain.Unload(appDomain);
            }
        }

        private static void CurrentDomainOnDomainUnload(object sender,
                                                        EventArgs eventArgs)
        {
            Logger.Info("Manager.IntegrationTest.Console.Host.CurrentDomainOnDomainUnload.");

            QuitEvent.Set();
        }

        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

        private static void Console_CancelKeyPress(object sender,
                                                   ConsoleCancelEventArgs e)
        {
            QuitEvent.Set();

            e.Cancel = true;
        }

        public static FileInfo CreateNodeConfigFile(FileInfo nodeConfigurationFile,
                                                    string newConfigurationFileName,
                                                    string nodeName,
                                                    Uri managerEndpoint,
                                                    Uri nodeEndPoint,
                                                    string handlerAssembly)
        {
            var copiedNodeConfigFile = nodeConfigurationFile.CopyTo(newConfigurationFileName,
                                                                    true);

            // Change app settings.
            var configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = copiedNodeConfigFile.FullName
            };

            var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap,
                                                                         ConfigurationUserLevel.None);

            config.AppSettings.Settings["NodeName"].Value = nodeName;
            config.AppSettings.Settings["BaseAddress"].Value = nodeEndPoint.ToString();
            config.AppSettings.Settings["ManagerLocation"].Value = managerEndpoint.ToString();
            config.AppSettings.Settings["HandlerAssembly"].Value = handlerAssembly;

            config.Save();

            return copiedNodeConfigFile;
        }

        public static FileInfo CopyManagerConfigurationFile(FileInfo managerConfigFile,
                                                            string newConfigFileName)
        {
            return managerConfigFile.CopyTo(newConfigFileName,
                                            true);
        }
    }
}