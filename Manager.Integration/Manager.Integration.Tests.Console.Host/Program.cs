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
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using log4net.Config;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Properties;
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
            AppDomains.AddOrUpdate(key,
                                   value,
                                   (name,
                                    val) => value);
        }


        private static void StartSelfHosting()
        {
            Configuration configuration =
                new Configuration(Settings.Default.ManagerIntegrationTestControllerBaseAddress);

            using (WebApp.Start(configuration.BaseAddress.ToString(),
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
                                                "Started listening on port : ( " + configuration.BaseAddress + " )");

                QuitEvent.WaitOne();
            }
        }

        private static int NumberOfNodesToStart { get; set; }

        private static Dictionary<string, FileInfo> NodeconfigurationFiles { get; set; }

        public static void Main(string[] args)
        {
            LogHelper.LogInfoWithLineNumber(Logger,
                                            "Start.");

            CurrentDomainConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            XmlConfigurator.ConfigureAndWatch(new FileInfo(CurrentDomainConfigurationFile));

            SetConsoleCtrlHandler(ConsoleCtrlCheck,
                                  true);

            AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            AppDomains = new ConcurrentDictionary<string, AppDomain>();

            CurrentDomainEvidence = AppDomain.CurrentDomain.Evidence;

            LogHelper.LogInfoWithLineNumber(Logger,
                                            "started.");

            DirectoryManagerConfigurationFileFullPath =
                new DirectoryInfo(AddEndingSlash(Settings.Default.ManagerConfigurationFileFullPath + _buildMode));

            ManagerConfigurationFile =
                new FileInfo(DirectoryManagerConfigurationFileFullPath.FullName +
                             Settings.Default.ManagerConfigurationFileName);

            CopiedManagerConfigurationFile =
                CopyManagerConfigurationFile(ManagerConfigurationFile,
                                             CopiedManagerConfigName);

            new Task(StartNewManager).Start();

            DirectoryNodeConfigurationFileFullPath =
                new DirectoryInfo(AddEndingSlash(Settings.Default.NodeConfigurationFileFullPath + _buildMode));

            NodeConfigurationFile =
                new FileInfo(DirectoryNodeConfigurationFileFullPath.FullName +
                             Settings.Default.NodeConfigurationFileName);

            NodeconfigurationFiles = new Dictionary<string, FileInfo>();

            PortStartNumber =
                Settings.Default.NodeEndpointPortNumberStart;

            NumberOfNodesToStart = Settings.Default.NumberOfNodesToStart;

            if (args.Any())
            {
                LogHelper.LogInfoWithLineNumber(Logger,
                                                "Has command arguments.");

                NumberOfNodesToStart = Convert.ToInt32(args[0]);
            }

            LogHelper.LogInfoWithLineNumber(Logger,
                                            NumberOfNodesToStart + " number of nodes will be started.");

            if (NumberOfNodesToStart > 0)
            {
                for (var i = 1; i <= NumberOfNodesToStart; i++)
                {
                    CreateNodeConfigurationFile(i);
                }
            }

            DirectoryNodeAssemblyLocationFullPath =
                new DirectoryInfo(AddEndingSlash(Settings.Default.NodeAssemblyLocationFullPath + _buildMode));

            foreach (var nodeconfigurationFile in NodeconfigurationFiles)
            {
                new Task(() => { CreateNodeAppDomain(nodeconfigurationFile); }).Start();
            }

            StartSelfHosting();
        }

        private static Evidence CurrentDomainEvidence { get; set; }

        private static void StartNewManager()
        {
            DirectoryManagerAssemblyLocationFullPath =
                new DirectoryInfo(AddEndingSlash(Settings.Default.ManagerAssemblyLocationFullPath + _buildMode));

            // Start manager.
            var managerAppDomainSetup = new AppDomainSetup
            {
                ApplicationBase = DirectoryManagerAssemblyLocationFullPath.FullName,
                ApplicationName = Settings.Default.ManagerAssemblyName,
                ShadowCopyFiles = "true",
                ConfigurationFile = CopiedManagerConfigurationFile.FullName
            };

            AppDomain managerAppDomain = AppDomain.CreateDomain(managerAppDomainSetup.ApplicationName,
                                                                CurrentDomainEvidence,
                                                                managerAppDomainSetup);

            AddOrUpdateAppDomains(CopiedManagerConfigurationFile.Name,
                                  managerAppDomain);

            var assemblyFile = new FileInfo(Path.Combine(managerAppDomainSetup.ApplicationBase,
                                                         managerAppDomainSetup.ApplicationName));

            LogHelper.LogInfoWithLineNumber(Logger,
                                            "Manager (appdomain) will start with friendly name : " + managerAppDomain.FriendlyName);

            managerAppDomain.ExecuteAssembly(assemblyFile.FullName);
        }

        public static DirectoryInfo DirectoryManagerAssemblyLocationFullPath { get; set; }

        private static void CreateNodeAppDomain(KeyValuePair<string, FileInfo> nodeconfigurationFile)
        {
            var nodeAppDomainSetup = new AppDomainSetup
            {
                ApplicationBase = DirectoryNodeAssemblyLocationFullPath.FullName,
                ApplicationName = Settings.Default.NodeAssemblyName,
                ShadowCopyFiles = "true",
                ConfigurationFile = nodeconfigurationFile.Value.FullName
            };

            var nodeAppDomain = AppDomain.CreateDomain(nodeconfigurationFile.Key,
                                                       CurrentDomainEvidence,
                                                       nodeAppDomainSetup);

            AddOrUpdateAppDomains(nodeconfigurationFile.Value.Name,
                                  nodeAppDomain);

            var assemblyToExecute =
                new FileInfo(Path.Combine(nodeAppDomainSetup.ApplicationBase,
                                          nodeAppDomainSetup.ApplicationName));

            LogHelper.LogInfoWithLineNumber(Logger,
                                            "Node (appdomain) will start with friendly name : " + nodeAppDomain.FriendlyName);

            nodeAppDomain.ExecuteAssembly(assemblyToExecute.FullName);
        }

        private static DirectoryInfo DirectoryNodeAssemblyLocationFullPath { get; set; }

        private static string CurrentDomainConfigurationFile { get; set; }

        private static DirectoryInfo DirectoryManagerConfigurationFileFullPath { get; set; }

        private static DirectoryInfo DirectoryNodeConfigurationFileFullPath { get; set; }

        private static FileInfo NodeConfigurationFile { get; set; }

        private static int PortStartNumber { get; set; }

        private static void CreateNodeConfigurationFile(int i)
        {
            var nodeName = "Node" + i;

            var configName = nodeName + ".config";

            var portNumber = PortStartNumber + (i - 1);

            var endPointUri =
                new Uri(Settings.Default.NodeEndpointUriTemplate.Replace("PORTNUMBER",
                                                                         portNumber.ToString()));

            var copiedConfigurationFile =
                CreateNodeConfigurationFile(NodeConfigurationFile,
                                            configName,
                                            nodeName,
                                            new Uri(Settings.Default.ManagerLocationUri),
                                            endPointUri,
                                            Settings.Default.HandlerAssembly);

            NodeconfigurationFiles.Add(nodeName,
                                       copiedConfigurationFile);
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
            LogHelper.LogInfoWithLineNumber(Logger,
                                            "Start CurrentDomainOnDomainUnload.");

            if (AppDomains.Any())
            {
                LogHelper.LogInfoWithLineNumber(Logger,
                                                "Start unloading app domains.");

                foreach (var appDomain in AppDomains.Values)
                {
                    string friendlyName = appDomain.FriendlyName;
                    LogHelper.LogInfoWithLineNumber(Logger,
                                                        "Try unload appdomain with friendly name : " + friendlyName);
                    try
                    {
                        AppDomain.Unload(appDomain);

                        LogHelper.LogInfoWithLineNumber(Logger,
                                                        "Unload appdomain with friendly name : " + friendlyName);
                    }

                    catch (AppDomainUnloadedException appDomainUnloadedException)
                    {
                        LogHelper.LogWarningWithLineNumber(Logger,
                                                           appDomainUnloadedException.Message);
                    }

                    catch (Exception exp)
                    {
                        LogHelper.LogErrorWithLineNumber(Logger,
                                                         exp.Message,
                                                         exp);
                    }
                }


                LogHelper.LogInfoWithLineNumber(Logger,
                                                "Finished unloading app domains.");
            }

            QuitEvent.Set();

            LogHelper.LogInfoWithLineNumber(Logger,
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

        public static void ShutDownNode(string id)
        {
            KeyValuePair<string, AppDomain> appDomainToUnload = AppDomains.FirstOrDefault(pair => pair.Key == id);
            
            if (appDomainToUnload.Value != null)
            {
                string friendlyName = appDomainToUnload.Value.FriendlyName;

                try
                {                    
                    LogHelper.LogInfoWithLineNumber(Logger,"Try unload appdomain with friendly name : " + friendlyName);

                    AppDomain.Unload(appDomainToUnload.Value);

                    LogHelper.LogInfoWithLineNumber(Logger,"Unload appdomain with friendly name : " + friendlyName);
                }

                catch (AppDomainUnloadedException appDomainUnloadedException)
                {
                    LogHelper.LogWarningWithLineNumber(Logger,
                                                       appDomainUnloadedException.Message);
                }

                catch (Exception exp)
                {
                    LogHelper.LogErrorWithLineNumber(Logger,
                                                     exp.Message,
                                                     exp);
                }

                //AddOrUpdateAppDomains(id,
                //                      null);
            }
            try
            {
                AppDomain tmp = AppDomains.Values.FirstOrDefault();
                if (tmp != null)
                    LogHelper.LogInfoWithLineNumber(Logger,"The appDomain " + tmp.FriendlyName + "is not unloaded!");
            }
            catch
            {
                LogHelper.LogInfoWithLineNumber(Logger,"The appDomain is unloaded!");
            }
        }

        public static bool StartNewNode()
        {
            return true;
        }

        public static bool NodeExists(string id)
        {
            return true;
        }
    }
}