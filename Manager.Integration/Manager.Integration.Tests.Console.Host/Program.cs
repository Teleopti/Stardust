using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using log4net.Config;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.LoadBalancer;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;
using Manager.IntegrationTest.Console.Host.Properties;
using Manager.IntegrationTest.Console.Host.Tasks;
using Microsoft.Owin.Hosting;
using Owin;
using Configuration = Manager.IntegrationTest.Console.Host.Models.Configuration;

namespace Manager.IntegrationTest.Console.Host
{
	public static class Program
	{
		private const string ManagersCommandArgument = "Managers";

		private const string NodesCommandArgument = "Nodes";


#if (DEBUG)
		private static readonly string _buildMode = "Debug";
#else
        private static string _buildMode = "Release";
#endif

		private static readonly ILog Logger = LogManager.GetLogger(typeof (Program));

		public static FileInfo ManagerConfigurationFile { get; set; }

		public static Dictionary<Uri, FileInfo> CopiedManagerConfigurationFiles { get; set; }

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
			var configuration =
				new Configuration(Settings.Default.IntegrationControllerBaseAddress);

			var address =
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
				Logger.LogInfoWithLineNumber("Started listening on port : ( " + address + " )");

				QuitEvent.WaitOne();
			}
		}

		private static void StartLoadBalancerProxy(IEnumerable<Uri> managerUriList)
		{
			Logger.LogDebugWithLineNumber("Start.");

			var configuration = new Configuration(Settings.Default.ManagerLocationUri);

			var address =
				configuration.BaseAddress.Scheme + "://+:9000/StardustDashboard";

			RoundRobin.Set(managerUriList.ToList());

			using (WebApp.Start<LoadBalancerStartup>(address))
			{
				Logger.LogInfoWithLineNumber("Load balancer started listening on port : ( " + address + " )");

				QuitEvent.WaitOne();
			}
		}


		private static int NumberOfManagersToStart { get; set; }

		private static int NumberOfNodesToStart { get; set; }

		private static Dictionary<string, FileInfo> NodeconfigurationFiles { get; set; }

		public static void Main(string[] args)
		{
			CurrentDomainConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(CurrentDomainConfigurationFile));

			Logger.LogDebugWithLineNumber("Start.");

			SetConsoleCtrlHandler(ConsoleCtrlCheck,
			                      true);

			//---------------------------------------------------------
			// Number of managers and number of nodes to start up.
			//---------------------------------------------------------
			NumberOfManagersToStart = Settings.Default.NumberOfManagersToStart;
			NumberOfNodesToStart = Settings.Default.NumberOfNodesToStart;

			if (args.Any())
			{
				Logger.LogDebugWithLineNumber("Has command arguments.");

				foreach (var s in args)
				{
					var values = s.Split('=');

					// Managers.
					if (values[0].Equals(ManagersCommandArgument,
					                     StringComparison.InvariantCultureIgnoreCase))
					{
						NumberOfManagersToStart = Convert.ToInt32(values[1]);
					}

					// Nodes.
					if (values[0].Equals(NodesCommandArgument,
					                     StringComparison.InvariantCultureIgnoreCase))
					{
						NumberOfNodesToStart = Convert.ToInt32(values[1]);
					}
				}
			}

			Logger.LogInfoWithLineNumber(NumberOfManagersToStart + " number of managers will be started.");

			Logger.LogInfoWithLineNumber(NumberOfNodesToStart + " number of nodes will be started.");


			Logger.LogDebugWithLineNumber("AppDomain.CurrentDomain.DomainUnload");
			AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;

			Logger.LogDebugWithLineNumber("AppDomain.CurrentDomain.UnhandledException");
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			DirectoryManagerAssemblyLocationFullPath =
				new DirectoryInfo(Path.Combine(Settings.Default.ManagerAssemblyLocationFullPath,
				                               _buildMode));

			DirectoryManagerConfigurationFileFullPath =
				new DirectoryInfo(Path.Combine(Settings.Default.ManagerConfigurationFileFullPath,
				                               _buildMode));

			Logger.LogDebugWithLineNumber("DirectoryManagerConfigurationFileFullPath : " +
			                                 DirectoryManagerConfigurationFileFullPath.FullName);


			ManagerConfigurationFile =
				new FileInfo(Path.Combine(DirectoryManagerConfigurationFileFullPath.FullName,
				                          Settings.Default.ManagerConfigurationFileName));

			Logger.LogDebugWithLineNumber("ManagerConfigurationFile : " + ManagerConfigurationFile.FullName);

			CopiedManagerConfigurationFiles = new Dictionary<Uri, FileInfo>();

			for (var i = 0; i < NumberOfManagersToStart; i++)
			{
				var portNumber = Settings.Default.ManagerEndpointPortNumberStart + i;
				var allowedDowntimeSeconds = Settings.Default.AllowedDowntimeSeconds;

				Uri uri;

				var copiedManagerConfigurationFile =
					CopyManagerConfigurationFile(ManagerConfigurationFile,
					                             i + 1,
					                             portNumber,
					                             allowedDowntimeSeconds,
					                             out uri);

				CopiedManagerConfigurationFiles.Add(uri,
				                                    copiedManagerConfigurationFile);
			}

			Logger.LogDebugWithLineNumber("Created " + CopiedManagerConfigurationFiles.Count + " manager configuration files.");


			DirectoryNodeConfigurationFileFullPath =
				new DirectoryInfo(Path.Combine(Settings.Default.NodeConfigurationFileFullPath,
				                               _buildMode));

			Logger.LogDebugWithLineNumber("DirectoryNodeConfigurationFileFullPath : " +
			                                 DirectoryNodeConfigurationFileFullPath.FullName);

			NodeConfigurationFile =
				new FileInfo(Path.Combine(DirectoryNodeConfigurationFileFullPath.FullName,
				                          Settings.Default.NodeConfigurationFileName));

			Logger.LogDebugWithLineNumber("NodeConfigurationFile : " + NodeConfigurationFile.FullName);

			NodeconfigurationFiles = new Dictionary<string, FileInfo>();

			PortStartNumber =
				Settings.Default.NodeEndpointPortNumberStart;

			if (NumberOfNodesToStart > 0)
			{
				for (var i = 1; i <= NumberOfNodesToStart; i++)
				{
					Logger.LogDebugWithLineNumber("Start creating node configuration file for node id : " + i);

					var nodeConfig = CreateNodeConfigurationFile(i);

					Logger.LogDebugWithLineNumber("Finished creating node configuration file for node : ( id, config file ) : ( " +
					                                 i + ", " + nodeConfig.FullName + " )");
				}
			}

			//-------------------------------------------------------
			// App domain manager tasks.
			//-------------------------------------------------------
			Logger.LogDebugWithLineNumber("AppDomainManagerTasks");

			AppDomainManagerTasks = new ConcurrentDictionary<string, AppDomainManagerTask>();
			 
			Parallel.ForEach(CopiedManagerConfigurationFiles.Values, copiedManagerConfigurationFile =>
			{
				var appDomainManagerTask =
					new AppDomainManagerTask(_buildMode,
					                         DirectoryManagerAssemblyLocationFullPath,
					                         copiedManagerConfigurationFile,
					                         Settings.Default.ManagerAssemblyName);

				Logger.LogDebugWithLineNumber("Start: AppDomainManagerTask.StartTask");

				appDomainManagerTask.StartTask(new CancellationTokenSource());

				AppDomainManagerTasks.AddOrUpdate(copiedManagerConfigurationFile.Name, 
												  appDomainManagerTask,
												  (s, task) => appDomainManagerTask);
			});

			Logger.LogDebugWithLineNumber("Finished: AppDomainManagerTask.StartTask");

			//-------------------------------------------------------
			//-------------------------------------------------------
			DirectoryNodeAssemblyLocationFullPath =
				new DirectoryInfo(Path.Combine(Settings.Default.NodeAssemblyLocationFullPath,
				                               _buildMode));

			Logger.LogDebugWithLineNumber("DirectoryNodeAssemblyLocationFullPath : " +
			                                 DirectoryNodeAssemblyLocationFullPath.FullName);

			AppDomainNodeTasks = new ConcurrentDictionary<string, AppDomainNodeTask>();

			Parallel.ForEach(NodeconfigurationFiles, pair =>
			{
				Logger.LogDebugWithLineNumber("AppDomainNodeTask");

				var appDomainNodeTask =
					new AppDomainNodeTask(_buildMode,
					                      DirectoryNodeAssemblyLocationFullPath,
					                      pair.Value,
					                      Settings.Default.NodeAssemblyName);

				Logger.LogDebugWithLineNumber("Start : AppDomainNodeTask.StartTask");

				appDomainNodeTask.StartTask(new CancellationTokenSource());

				Logger.LogDebugWithLineNumber("Finished : AppDomainNodeTask.StartTask");

				AppDomainNodeTasks.AddOrUpdate(pair.Value.Name,
											  appDomainNodeTask,
											  (s, task) => appDomainNodeTask);
			});

			Task.Factory.StartNew(() => StartLoadBalancerProxy(CopiedManagerConfigurationFiles.Keys));

			StartSelfHosting();
		}

		private static ConcurrentDictionary<string,AppDomainNodeTask> AppDomainNodeTasks { get; set; }

		public static ConcurrentDictionary<string,AppDomainManagerTask> AppDomainManagerTasks { get; set; }

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
			var pingToManagerSeconds = Settings.Default.PingToManagerSeconds;

			var copiedConfigurationFile =
				CreateNodeConfigurationFile(NodeConfigurationFile,
				                            configName,
				                            nodeName,
				                            endPointUri,
				                            pingToManagerSeconds,
				                            Settings.Default.HandlerAssembly);

			NodeconfigurationFiles.Add(nodeName,
			                           copiedConfigurationFile);

			return copiedConfigurationFile;
		}

		private static void CurrentDomain_UnhandledException(object sender,
		                                                     UnhandledExceptionEventArgs e)
		{
			var exp = e.ExceptionObject as Exception;

			if (exp != null)
			{
				Logger.LogFatalWithLineNumber(exp.Message,
				                                 exp);
			}
		}

		private static void CurrentDomainOnDomainUnload(object sender,
		                                                EventArgs eventArgs)
		{
			Logger.LogDebugWithLineNumber("Start CurrentDomainOnDomainUnload.");

			//-------------------------------------------
			// Shut down nodes.
			//-------------------------------------------
			foreach (var appDomainNodeTask in AppDomainNodeTasks)
			{
				appDomainNodeTask.Value.Dispose();
			}

			//-------------------------------------------
			// Shut down managers.
			//-------------------------------------------
			foreach (var appDomainManagerTask in AppDomainManagerTasks)
			{
				appDomainManagerTask.Value.Dispose();
			}

			QuitEvent.Set();

			Logger.LogDebugWithLineNumber("Finished CurrentDomainOnDomainUnload.");
		}


		private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);


		public static FileInfo CreateNodeConfigurationFile(FileInfo nodeConfigurationFile,
		                                                   string newConfigurationFileName,
		                                                   string nodeName,
		                                                   Uri nodeEndPoint,
		                                                   int pingToManagerSeconds,
		                                                   string handlerAssembly)
		{
			//-------------------------------------------------------
			// Create Node configuration file.
			//-------------------------------------------------------
			var copiedNodeConfigFile = nodeConfigurationFile.CopyTo(newConfigurationFileName,
			                                                        true);

			var nodeExeConfigurationFileMap = new ExeConfigurationFileMap
			{
				ExeConfigFilename = copiedNodeConfigFile.FullName
			};

			var nodeConfig =
				ConfigurationManager.OpenMappedExeConfiguration(nodeExeConfigurationFileMap,
				                                                ConfigurationUserLevel.None);

			nodeConfig.AppSettings.Settings["NodeName"].Value = nodeName;
			nodeConfig.AppSettings.Settings["BaseAddress"].Value = nodeEndPoint.ToString();
			nodeConfig.AppSettings.Settings["ManagerLocation"].Value = Settings.Default.ManagerLocationUri;
			nodeConfig.AppSettings.Settings["HandlerAssembly"].Value = handlerAssembly;
			nodeConfig.AppSettings.Settings["PingToManagerSeconds"].Value = pingToManagerSeconds.ToString();

			nodeConfig.Save();

			return copiedNodeConfigFile;
		}

		public static FileInfo CopyManagerConfigurationFile(FileInfo managerConfigFile,
		                                                    int i,
		                                                    int portNumber,
		                                                    int allowedDowntimeSeconds,
		                                                    out Uri uri)
		{
			var newConfigFileName = "Manager" + i + ".config";

			var copyManagerConfigurationFile = managerConfigFile.CopyTo(newConfigFileName,
			                                                            true);

			var configFileMap = new ExeConfigurationFileMap
			{
				ExeConfigFilename = copyManagerConfigurationFile.FullName
			};

			var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap,
			                                                             ConfigurationUserLevel.None);
			var uriBuilder =
				new UriBuilder(config.AppSettings.Settings["BaseAddress"].Value)
				{
					Port = portNumber
				};

			uri = uriBuilder.Uri;

			config.AppSettings.Settings["ManagerName"].Value = "Manager" + i;
			config.AppSettings.Settings["BaseAddress"].Value = uri.ToString();
			config.AppSettings.Settings["AllowedNodeDownTimeSeconds"].Value = allowedDowntimeSeconds.ToString();
			config.Save();

			return copyManagerConfigurationFile;
		}

		public static bool ShutDownManager(string friendlyName)
		{
			var ret = false;

			Logger.LogDebugWithLineNumber("Started.");

			if (AppDomainManagerTasks != null && AppDomainManagerTasks.Any())
			{
				AppDomainManagerTask managerToDispose;

				ret = AppDomainManagerTasks.TryRemove(friendlyName, out managerToDispose);


				if (ret)
				{
					Logger.LogDebugWithLineNumber("Start disposing manager (appdomain) with friendly name :" + friendlyName);

					managerToDispose.Dispose();


					Logger.LogDebugWithLineNumber("Finished to dispose manager (appdomain) with friendly name :" + friendlyName);

				}
			}

			Logger.LogDebugWithLineNumber("Finished.");

			return ret;
		}

		public static bool ShutDownNode(string friendlyName)
		{
			var ret = false;

			Logger.LogDebugWithLineNumber("Started.");

			if (AppDomainNodeTasks != null && AppDomainNodeTasks.Any())
			{
				AppDomainNodeTask nodeToDispose;

				ret = AppDomainNodeTasks.TryRemove(friendlyName, out nodeToDispose);

				if (nodeToDispose != null)
				{
					Logger.LogDebugWithLineNumber("Start to dispose appdomain with friendly name :" + friendlyName);

					nodeToDispose.Dispose();

					Logger.LogDebugWithLineNumber("Finished to dispose appdomain with friendly name :" + friendlyName);

					ret = true;
				}
			}

			Logger.LogDebugWithLineNumber("Finished.");

			return ret;
		}

		public static void StartNewNode(out string friendlyName)
		{
			friendlyName = null;

			NumberOfNodesToStart++;

			if (NumberOfNodesToStart > 0)
			{
				Logger.LogDebugWithLineNumber("Start creating node configuration file for node id : " + NumberOfNodesToStart);

				var nodeConfig = CreateNodeConfigurationFile(NumberOfNodesToStart);

				friendlyName = nodeConfig.Name;

				Logger.LogDebugWithLineNumber("AppDomainNodeTask");

				var appDomainNodeTask =
					new AppDomainNodeTask(_buildMode,
					                      DirectoryNodeAssemblyLocationFullPath,
					                      nodeConfig,
					                      Settings.Default.NodeAssemblyName);

				Logger.LogDebugWithLineNumber("Start : AppDomainNodeTask.StartTask");

				appDomainNodeTask.StartTask(new CancellationTokenSource());

				Logger.LogDebugWithLineNumber("Finished : AppDomainNodeTask.StartTask");

				AppDomainNodeTasks.AddOrUpdate(appDomainNodeTask.GetAppDomainUniqueId(), 
											   appDomainNodeTask,
											   (s, task) => appDomainNodeTask);

				Logger.LogDebugWithLineNumber("Finished creating node configuration file for node : ( id, config file ) : ( " +
				                                 NumberOfNodesToStart + ", " + nodeConfig.FullName + " )");
			}
		}

		public static List<string> GetAllmanagers()
		{
			return AppDomainManagerTasks.Keys.ToList();
		}

		public static List<string> GetAllNodes()
		{
			return AppDomainNodeTasks.Keys.ToList();
		}

		public static void StartNewManager(out string friendlyname)
		{
			NumberOfManagersToStart++;

			var portNumber =
				Settings.Default.ManagerEndpointPortNumberStart + (NumberOfManagersToStart - 1);
			var allowedDowntimeSeconds = Settings.Default.AllowedDowntimeSeconds;

			Uri uri;

			var copiedManagerConfigurationFile =
				CopyManagerConfigurationFile(ManagerConfigurationFile,
				                             NumberOfManagersToStart + 1,
				                             portNumber,
				                             allowedDowntimeSeconds,
				                             out uri);

			CopiedManagerConfigurationFiles.Add(uri,
			                                    copiedManagerConfigurationFile);

			var appDomainManagerTask =
				new AppDomainManagerTask(_buildMode,
				                         DirectoryManagerAssemblyLocationFullPath,
				                         copiedManagerConfigurationFile,
				                         Settings.Default.ManagerAssemblyName);

			AppDomainManagerTasks.AddOrUpdate(appDomainManagerTask.GetAppDomainUniqueId(), 
											 appDomainManagerTask,
											 (s, task) => appDomainManagerTask);

			Logger.LogDebugWithLineNumber("Start: AppDomainManagerTask.StartTask");

			appDomainManagerTask.StartTask(new CancellationTokenSource());

			Logger.LogDebugWithLineNumber("Finished: AppDomainManagerTask.StartTask");

			friendlyname = appDomainManagerTask.GetAppDomainUniqueId();
		}
	}
}