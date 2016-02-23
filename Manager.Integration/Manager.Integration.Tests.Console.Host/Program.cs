using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using log4net.Config;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Properties;
using Manager.IntegrationTest.Console.Host.Tasks;
using Microsoft.Owin.Hosting;
using Owin;
using Configuration = Manager.IntegrationTest.Console.Host.Models.Configuration;

namespace Manager.IntegrationTest.Console.Host
{
    public static class Program
    {
        private const string CopiedManagerConfigName = "Manager.config";

#if (DEBUG)
        private static string _buildMode = "Debug";
#else
        private static string _buildMode = "Release";
#endif

        private static readonly ILog Logger = LogManager.GetLogger(typeof (Program));

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
                QuitEvent.Set();

                return true;
            }

            return false;
        }

        private static void StartSelfHosting()
        {
            Configuration configuration =
                new Configuration(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

            string address =
                configuration.BaseAddress.Scheme + "://+:" + configuration.BaseAddress.Port + "/";

            using (WebApp.Start(address,
                                appBuilder =>
                                {
                                    var builder = new ContainerBuilder();

                                    var container = builder.Build();

                                    var config = new HttpConfiguration
                                    {
                                        DependencyResolver = new AutofacWebApiDependencyResolver(container)
                                    };

                                    config.MapHttpAttributeRoutes();

                                    appBuilder.UseAutofacMiddleware(container);
                                    appBuilder.UseAutofacWebApi(config);
                                    appBuilder.UseWebApi(config);
                                }))
            {
                LogHelper.LogInfoWithLineNumber(Logger,
                                                "Started listening on port : ( " + address + " )");

                QuitEvent.WaitOne();
            }
        }

        private static int NumberOfNodesToStart { get; set; }

        private static Dictionary<string, FileInfo> NodeconfigurationFiles { get; set; }

        public static void Main(string[] args)
        {
            CurrentDomainConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(CurrentDomainConfigurationFile));

            LogHelper.LogDebugWithLineNumber(Logger,
                                            "Start.");


            SetConsoleCtrlHandler(ConsoleCtrlCheck,
                                  true);

            LogHelper.LogDebugWithLineNumber(Logger,
                                             "AppDomain.CurrentDomain.DomainUnload");
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;

            LogHelper.LogDebugWithLineNumber(Logger,
                                             "AppDomain.CurrentDomain.UnhandledException");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            DirectoryManagerConfigurationFileFullPath =
                new DirectoryInfo(Path.Combine(Settings.Default.ManagerConfigurationFileFullPath,
                                               _buildMode));

            LogHelper.LogDebugWithLineNumber(Logger,
                                             "DirectoryManagerConfigurationFileFullPath : " + DirectoryManagerConfigurationFileFullPath.FullName);


            ManagerConfigurationFile =
                new FileInfo(Path.Combine(DirectoryManagerConfigurationFileFullPath.FullName,
                                          Settings.Default.ManagerConfigurationFileName));

            LogHelper.LogDebugWithLineNumber(Logger,
                                             "ManagerConfigurationFile : " + ManagerConfigurationFile.FullName);

            CopiedManagerConfigurationFile =
                CopyManagerConfigurationFile(ManagerConfigurationFile,
                                             CopiedManagerConfigName);

            LogHelper.LogDebugWithLineNumber(Logger,
                                             "CopiedManagerConfigurationFile : " + CopiedManagerConfigurationFile.FullName);

            DirectoryNodeConfigurationFileFullPath =
                new DirectoryInfo(Path.Combine(Settings.Default.NodeConfigurationFileFullPath,
                                               _buildMode));

            LogHelper.LogDebugWithLineNumber(Logger,
                                             "DirectoryNodeConfigurationFileFullPath : " + DirectoryNodeConfigurationFileFullPath.FullName);

            NodeConfigurationFile =
                new FileInfo(Path.Combine(DirectoryNodeConfigurationFileFullPath.FullName,
                                          Settings.Default.NodeConfigurationFileName));

            LogHelper.LogDebugWithLineNumber(Logger,
                                             "NodeConfigurationFile : " + NodeConfigurationFile.FullName);

            NodeconfigurationFiles = new Dictionary<string, FileInfo>();

            PortStartNumber =
                Settings.Default.NodeEndpointPortNumberStart;

            NumberOfNodesToStart = Settings.Default.NumberOfNodesToStart;

            if (args.Any())
            {
                LogHelper.LogDebugWithLineNumber(Logger,
                                                "Has command arguments.");

                NumberOfNodesToStart = Convert.ToInt32(args[0]);
            }

            LogHelper.LogInfoWithLineNumber(Logger,
                                            NumberOfNodesToStart + " number of nodes will be started.");

            if (NumberOfNodesToStart > 0)
            {
                for (var i = 1; i <= NumberOfNodesToStart; i++)
                {
                    LogHelper.LogDebugWithLineNumber(Logger,
                                                    "Start creating node configuration file for node id : " + i);

                    var nodeConfig = CreateNodeConfigurationFile(i);

                    LogHelper.LogDebugWithLineNumber(Logger,
                                                    "Finished creating node configuration file for node : ( id, config file ) : ( " + i + ", " + nodeConfig.FullName + " )");
                }
            }

            LogHelper.LogDebugWithLineNumber(Logger,
                                             "AppDomainManagerTask");

            AppDomainManagerTask =
                new AppDomainManagerTask(_buildMode,
                                         DirectoryManagerAssemblyLocationFullPath,
                                         CopiedManagerConfigurationFile,
                                         Settings.Default.ManagerAssemblyName);

            LogHelper.LogDebugWithLineNumber(Logger,
                                             "Start: AppDomainManagerTask.StartTask");

            AppDomainManagerTask.StartTask(new CancellationTokenSource());

            LogHelper.LogDebugWithLineNumber(Logger,
                                             "Finished: AppDomainManagerTask.StartTask");

            DirectoryNodeAssemblyLocationFullPath =
                new DirectoryInfo(Path.Combine(Settings.Default.NodeAssemblyLocationFullPath,
                                               _buildMode));

            LogHelper.LogDebugWithLineNumber(Logger, 
                                             "DirectoryNodeAssemblyLocationFullPath : " + DirectoryNodeAssemblyLocationFullPath.FullName);

            AppDomainNodeTasks = new List<AppDomainNodeTask>();

            foreach (KeyValuePair<string, FileInfo> nodeconfigurationFile in NodeconfigurationFiles)
            {
                LogHelper.LogDebugWithLineNumber(Logger,
                                                 "AppDomainNodeTask");

                AppDomainNodeTask appDomainNodeTask =
                    new AppDomainNodeTask(_buildMode,
                                          DirectoryNodeAssemblyLocationFullPath,
                                          nodeconfigurationFile.Value,
                                          Settings.Default.NodeAssemblyName);

                LogHelper.LogDebugWithLineNumber(Logger,
                                                 "Start : AppDomainNodeTask.StartTask");

                appDomainNodeTask.StartTask(new CancellationTokenSource());

                LogHelper.LogDebugWithLineNumber(Logger,
                                                 "Finished : AppDomainNodeTask.StartTask");

                AppDomainNodeTasks.Add(appDomainNodeTask);

                // Wait 5 seconds for a new node to start up.
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }

            StartSelfHosting();
        }

        private static List<AppDomainNodeTask> AppDomainNodeTasks { get; set; }

        public static AppDomainManagerTask AppDomainManagerTask { get; set; }

        public static DirectoryInfo DirectoryManagerAssemblyLocationFullPath { get; set; }


        private static DirectoryInfo DirectoryNodeAssemblyLocationFullPath { get; set; }

        private static string CurrentDomainConfigurationFile { get; set; }

        private static DirectoryInfo DirectoryManagerConfigurationFileFullPath { get; set; }

        private static DirectoryInfo DirectoryNodeConfigurationFileFullPath { get; set; }

        private static FileInfo NodeConfigurationFile { get; set; }

        private static int PortStartNumber { get; set; }

        private static FileInfo CreateNodeConfigurationFile(int i)
        {
            var nodeName = "Node" + i;

            var configName = nodeName + ".config";

            var portNumber = PortStartNumber + (i - 1);

            var endPointUri =
                new Uri(Settings.Default.NodeEndpointUriTemplate.Replace("PORTNUMBER",
                                                                         portNumber.ToString()));

            FileInfo copiedConfigurationFile =
                CreateNodeConfigurationFile(NodeConfigurationFile,
                                            configName,
                                            nodeName,
                                            new Uri(Settings.Default.ManagerLocationUri),
                                            endPointUri,
                                            Settings.Default.HandlerAssembly);

            NodeconfigurationFiles.Add(nodeName,
                                       copiedConfigurationFile);

            return copiedConfigurationFile;
        }

        private static void CurrentDomain_UnhandledException(object sender,
                                                             UnhandledExceptionEventArgs e)
        {
            if (!e.IsTerminating)
            {
                Exception exp = e.ExceptionObject as Exception;

                LogHelper.LogErrorWithLineNumber(Logger,
                                                 string.Empty,
                                                 exp.InnerException);
            }
        }

        private static void CurrentDomainOnDomainUnload(object sender,
                                                        EventArgs eventArgs)
        {
            LogHelper.LogDebugWithLineNumber(Logger,
                                            "Start CurrentDomainOnDomainUnload.");

            foreach (var appDomainNodeTask in AppDomainNodeTasks)
            {
                appDomainNodeTask.Dispose();
            }

            AppDomainManagerTask.Dispose();

            QuitEvent.Set();

            LogHelper.LogDebugWithLineNumber(Logger,
                                            "Finished CurrentDomainOnDomainUnload.");
        }


        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);


        public static FileInfo CreateNodeConfigurationFile(FileInfo nodeConfigurationFile,
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


        public static void ShutDownNode(string friendlyName)
        {
            LogHelper.LogDebugWithLineNumber(Logger,
                                            "Started.");

            if (AppDomainManagerTask != null &&
                AppDomainManagerTask.GetAppDomainFriendlyName()
                    .Equals(friendlyName,
                            StringComparison.InvariantCultureIgnoreCase))
            {
                AppDomainManagerTask.Dispose();

                AppDomainManagerTask = null;
            }

            if (AppDomainNodeTasks != null && AppDomainNodeTasks.Any())
            {
                AppDomainNodeTask nodeToDispose = null;

                foreach (var appDomainNodeTask in AppDomainNodeTasks)
                {
                    if (appDomainNodeTask.GetAppDomainFriendlyName()
                        .Equals(friendlyName,
                                StringComparison.InvariantCultureIgnoreCase))
                    {
                        nodeToDispose = appDomainNodeTask;

                        break;
                    }
                }

                if (nodeToDispose != null)
                {
                    LogHelper.LogDebugWithLineNumber(Logger,
                                                    "Start to dispose appdomain with friendly name :" + friendlyName);

                    nodeToDispose.Dispose();

                    AppDomainNodeTasks.Remove(nodeToDispose);

                    LogHelper.LogDebugWithLineNumber(Logger,
                                                    "Finished to dispose appdomain with friendly name :" + friendlyName);
                }
            }

            LogHelper.LogDebugWithLineNumber(Logger,
                                            "Finished.");
        }

        public static bool StartNewNode()
        {
            return true;
        }

        public static bool NodeExists(string id)
        {
            return true;
        }

        public static List<string> GetInstantiatedAppDomains()
        {
            List<string> listToReturn = null;

            if (AppDomainManagerTask == null &&
                (AppDomainNodeTasks == null || !AppDomainNodeTasks.Any()))
            {
                return listToReturn;
            }

            listToReturn = new List<string>();

            if (AppDomainManagerTask != null)
            {
                listToReturn.Add(AppDomainManagerTask.GetAppDomainFriendlyName());
            }

            if (AppDomainNodeTasks != null)
            {
                foreach (var appDomainNodeTask in AppDomainNodeTasks)
                {
                    listToReturn.Add(appDomainNodeTask.GetAppDomainFriendlyName());
                }
            }

            return listToReturn;
        }
    }
}