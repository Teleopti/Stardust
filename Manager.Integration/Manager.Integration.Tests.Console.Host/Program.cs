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
using Manager.IntegrationTest.Console.Host.LoadBalancer;
using Manager.IntegrationTest.Console.Host.Log4Net;
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

		private const string UseLoadBalancerIfJustOneManagerArgument = "UseLoadBalancerIfJustOneManager";


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
				Logger.InfoWithLineNumber("Started listening on port : ( " + address + " )");

				QuitEvent.WaitOne();
			}
		}

		private static void StartLoadBalancerProxy(IEnumerable<Uri> managerUriList)
		{
			Logger.DebugWithLineNumber("Start.");

			var configuration = new Configuration(Settings.Default.ManagerLocationUri);

			var address =
				configuration.BaseAddress.Scheme + "://+:9000/StardustDashboard";

			RoundRobin.Set(managerUriList.ToList());

			using (WebApp.Start<LoadBalancerStartup>(address))
			{
				Logger.InfoWithLineNumber("Load balancer started listening on port : ( " + address + " )");

				QuitEvent.WaitOne();
			}
		}


		private static int NumberOfManagersToStart
		{
			get
			{
				lock (LockNumberOfManagersToStart)
				{
					return _numberOfManagersToStart;
				}				
			}

			set
			{
				lock (LockNumberOfManagersToStart)
				{
					_numberOfManagersToStart = value;
				}				
			}
		}

		private static readonly object LockNumberOfManagersToStart = new object();

		private static readonly object LockNumberOfNodesToStart =new object();

		private static int NumberOfNodesToStart
		{
			get
			{
				lock (LockNumberOfNodesToStart)
				{
					return _numberOfNodesToStart;
				}				
			}

			set
			{
				lock (LockNumberOfNodesToStart)
				{
					_numberOfNodesToStart = value;
				}				
			}
		}

		private static Dictionary<string, FileInfo> NodeconfigurationFiles { get; set; }

		public static void Main(string[] args)
		{
			CurrentDomainConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(CurrentDomainConfigurationFile));

			Logger.DebugWithLineNumber("Start.");

			SetConsoleCtrlHandler(ConsoleCtrlCheck,
			                      true);

			//---------------------------------------------------------
			// Number of managers and number of nodes to start up.
			//---------------------------------------------------------
			NumberOfManagersToStart = Settings.Default.NumberOfManagersToStart;
			NumberOfNodesToStart = Settings.Default.NumberOfNodesToStart;

			//---------------------------------------------------------
			// Use load balancer if only one mananager is used.
			//---------------------------------------------------------
			UseLoadBalancerIfJustOneManager = Settings.Default.UseLoadBalancerIfJustOneManager;

			if (args.Any())
			{
				Logger.DebugWithLineNumber("Has command arguments.");

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

					// Use load balancer.
					if (values[0].Equals(UseLoadBalancerIfJustOneManagerArgument,
										 StringComparison.InvariantCultureIgnoreCase))
					{
						UseLoadBalancerIfJustOneManager = Convert.ToBoolean(values[1]);
					}
				}
			}

			Logger.InfoWithLineNumber(NumberOfManagersToStart + " number of managers will be started.");

			Logger.InfoWithLineNumber(NumberOfNodesToStart + " number of nodes will be started.");


			Logger.DebugWithLineNumber("AppDomain.CurrentDomain.DomainUnload");
			AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;

			Logger.DebugWithLineNumber("AppDomain.CurrentDomain.UnhandledException");
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			DirectoryManagerAssemblyLocationFullPath =
				new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.ManagerAssemblyLocationFullPath,
				                               _buildMode));

			DirectoryManagerConfigurationFileFullPath =
				new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.ManagerConfigurationFileFullPath,
				                               _buildMode));

			Logger.DebugWithLineNumber("DirectoryManagerConfigurationFileFullPath : " +
			                           DirectoryManagerConfigurationFileFullPath.FullName);


			ManagerConfigurationFile =
				new FileInfo(Path.Combine(DirectoryManagerConfigurationFileFullPath.FullName,
				                          Settings.Default.ManagerConfigurationFileName));

			Logger.DebugWithLineNumber("ManagerConfigurationFile : " + ManagerConfigurationFile.FullName);

			CopiedManagerConfigurationFiles = new Dictionary<Uri, FileInfo>();

			var allowedDowntimeSeconds = Settings.Default.AllowedDowntimeSeconds;

			if (NumberOfManagersToStart == 1 && !UseLoadBalancerIfJustOneManager)
			{
				var configuration = 
					new Configuration(Settings.Default.ManagerLocationUri);

				Uri address = configuration.BaseAddress;

				Uri uri;

				var copiedManagerConfigurationFile =
					CopyManagerConfigurationFile(ManagerConfigurationFile,
												 1,
												 address.ToString(),
												 allowedDowntimeSeconds,
												 out uri);

				CopiedManagerConfigurationFiles.Add(uri,
													copiedManagerConfigurationFile);
			}
			else
			{
				for (var i = 0; i < NumberOfManagersToStart; i++)
				{
					var portNumber = Settings.Default.ManagerEndpointPortNumberStart + i;

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
			}

			Logger.DebugWithLineNumber("Created " + CopiedManagerConfigurationFiles.Count + " manager configuration files.");


			DirectoryNodeConfigurationFileFullPath =
				new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.NodeConfigurationFileFullPath,
				                               _buildMode));

			Logger.DebugWithLineNumber("DirectoryNodeConfigurationFileFullPath : " +
			                           DirectoryNodeConfigurationFileFullPath.FullName);

			NodeConfigurationFile =
				new FileInfo(Path.Combine(DirectoryNodeConfigurationFileFullPath.FullName,
				                          Settings.Default.NodeConfigurationFileName));

			Logger.DebugWithLineNumber("NodeConfigurationFile : " + NodeConfigurationFile.FullName);

			NodeconfigurationFiles = new Dictionary<string, FileInfo>();

			PortStartNumber =
				Settings.Default.NodeEndpointPortNumberStart;

			if (NumberOfNodesToStart > 0)
			{
				for (var i = 1; i <= NumberOfNodesToStart; i++)
				{
					Logger.DebugWithLineNumber("Start creating node configuration file for node id : " + i);

					var nodeConfig = CreateNodeConfigurationFile(i);

					Logger.DebugWithLineNumber("Finished creating node configuration file for node : ( id, config file ) : ( " +
					                           i + ", " + nodeConfig.FullName + " )");
				}
			}

			//-------------------------------------------------------
			// App domain manager tasks.
			//-------------------------------------------------------
			Logger.DebugWithLineNumber("AppDomainManagerTasks");

			AppDomainManagerTasks = new ConcurrentDictionary<string, AppDomainManagerTask>();

			Parallel.ForEach(CopiedManagerConfigurationFiles.Values, copiedManagerConfigurationFile =>
			{
				var appDomainManagerTask =
					new AppDomainManagerTask(_buildMode,
					                         DirectoryManagerAssemblyLocationFullPath,
					                         copiedManagerConfigurationFile,
					                         Settings.Default.ManagerAssemblyName);

				Logger.DebugWithLineNumber("Start: AppDomainManagerTask.StartTask");

				appDomainManagerTask.StartTask(new CancellationTokenSource());

				AppDomainManagerTasks.AddOrUpdate(copiedManagerConfigurationFile.Name,
				                                  appDomainManagerTask,
				                                  (s, task) => appDomainManagerTask);
			});

			Logger.DebugWithLineNumber("Finished: AppDomainManagerTask.StartTask");

			//-------------------------------------------------------
			//-------------------------------------------------------
			DirectoryNodeAssemblyLocationFullPath =
				new DirectoryInfo(Path.Combine(Settings.Default.NodeAssemblyLocationFullPath,
				                               _buildMode));

			Logger.DebugWithLineNumber("DirectoryNodeAssemblyLocationFullPath : " +
			                           DirectoryNodeAssemblyLocationFullPath.FullName);

			AppDomainNodeTasks = new ConcurrentDictionary<string, AppDomainNodeTask>();

			Parallel.ForEach(NodeconfigurationFiles, pair =>
			{
				Logger.DebugWithLineNumber("AppDomainNodeTask");

				var appDomainNodeTask =
					new AppDomainNodeTask(_buildMode,
					                      DirectoryNodeAssemblyLocationFullPath,
					                      pair.Value,
					                      Settings.Default.NodeAssemblyName);

				Logger.DebugWithLineNumber("Start : AppDomainNodeTask.StartTask");

				appDomainNodeTask.StartTask(new CancellationTokenSource());

				Logger.DebugWithLineNumber("Finished : AppDomainNodeTask.StartTask");

				AppDomainNodeTasks.AddOrUpdate(pair.Value.Name,
				                               appDomainNodeTask,
				                               (s, task) => appDomainNodeTask);
			});

			if (NumberOfManagersToStart > 1 || UseLoadBalancerIfJustOneManager)
			{
				Task.Factory.StartNew(() => StartLoadBalancerProxy(CopiedManagerConfigurationFiles.Keys));
			}			

			StartSelfHosting();
		}

		public static bool UseLoadBalancerIfJustOneManager { get; set; }

		private static ConcurrentDictionary<string, AppDomainNodeTask> AppDomainNodeTasks { get; set; }

		public static ConcurrentDictionary<string, AppDomainManagerTask> AppDomainManagerTasks { get; set; }

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
			
			var pingToManagerSeconds = Settings.Default.PingToManagerSeconds;

			var copiedConfigurationFile =
				CreateNodeConfigurationFile(NodeConfigurationFile,
				                            configName,
				                            nodeName,
				                            portNumber.ToString(),
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
				Logger.FatalWithLineNumber(exp.Message,
				                           exp);
			}
		}

		private static void CurrentDomainOnDomainUnload(object sender,
		                                                EventArgs eventArgs)
		{
			Logger.DebugWithLineNumber("Start CurrentDomainOnDomainUnload.");

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

			Logger.DebugWithLineNumber("Finished CurrentDomainOnDomainUnload.");
		}


		private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);
		private static int _numberOfNodesToStart;
		private static int _numberOfManagersToStart;


		public static FileInfo CreateNodeConfigurationFile(FileInfo nodeConfigurationFile,
		                                                   string newConfigurationFileName,
		                                                   string nodeName,
		                                                   string port,
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
			nodeConfig.AppSettings.Settings["Port"].Value = port;
			nodeConfig.AppSettings.Settings["ManagerLocation"].Value = Settings.Default.ManagerLocationUri;
			nodeConfig.AppSettings.Settings["HandlerAssembly"].Value = handlerAssembly;
			nodeConfig.AppSettings.Settings["PingToManagerSeconds"].Value = pingToManagerSeconds.ToString();

			nodeConfig.Save();

			return copiedNodeConfigFile;
		}

		public static FileInfo CopyManagerConfigurationFile(FileInfo managerConfigFile,
		                                                    int i,
															string baseAddress,
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

			config.AppSettings.Settings["ManagerName"].Value = "Manager" + i;
			config.AppSettings.Settings["BaseAddress"].Value = baseAddress;
			config.AppSettings.Settings["AllowedNodeDownTimeSeconds"].Value = allowedDowntimeSeconds.ToString();
			config.Save();

			uri = new Uri(baseAddress);

			return copyManagerConfigurationFile;
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

			Logger.DebugWithLineNumber("Started.");

			if (AppDomainManagerTasks != null && AppDomainManagerTasks.Any())
			{
				AppDomainManagerTask managerToDispose;

				ret = AppDomainManagerTasks.TryRemove(friendlyName, out managerToDispose);


				if (ret)
				{
					Logger.DebugWithLineNumber("Start disposing manager (appdomain) with friendly name :" + friendlyName);

					managerToDispose.Dispose();

					Logger.DebugWithLineNumber("Finished to dispose manager (appdomain) with friendly name :" + friendlyName);
				}
			}

			Logger.DebugWithLineNumber("Finished.");

			return ret;
		}

		public static bool ShutDownNode(string friendlyName)
		{
			var ret = false;

			Logger.DebugWithLineNumber("Started.");

			if (AppDomainNodeTasks != null && AppDomainNodeTasks.Any())
			{
				AppDomainNodeTask nodeToDispose;

				ret = AppDomainNodeTasks.TryRemove(friendlyName, out nodeToDispose);

				if (nodeToDispose != null)
				{
					Logger.DebugWithLineNumber("Start to dispose appdomain with friendly name :" + friendlyName);

					nodeToDispose.Dispose();

					Logger.DebugWithLineNumber("Finished to dispose appdomain with friendly name :" + friendlyName);

					ret = true;
				}
			}

			Logger.DebugWithLineNumber("Finished.");

			return ret;
		}

		public static void StartNewNode(out string friendlyName)
		{
			friendlyName = null;

			NumberOfNodesToStart++;

			if (NumberOfNodesToStart > 0)
			{
				Logger.DebugWithLineNumber("Start creating node configuration file for node id : " + NumberOfNodesToStart);

				var nodeConfig = CreateNodeConfigurationFile(NumberOfNodesToStart);

				friendlyName = nodeConfig.Name;

				Logger.DebugWithLineNumber("AppDomainNodeTask");

				var appDomainNodeTask =
					new AppDomainNodeTask(_buildMode,
					                      DirectoryNodeAssemblyLocationFullPath,
					                      nodeConfig,
					                      Settings.Default.NodeAssemblyName);

				Logger.DebugWithLineNumber("Start : AppDomainNodeTask.StartTask");

				appDomainNodeTask.StartTask(new CancellationTokenSource());

				Logger.DebugWithLineNumber("Finished : AppDomainNodeTask.StartTask");

				var appDomainUniqueId = appDomainNodeTask.GetAppDomainUniqueId();

				AppDomainNodeTasks.AddOrUpdate(appDomainUniqueId,
				                               appDomainNodeTask,
				                               (s, task) => appDomainNodeTask);

				Logger.DebugWithLineNumber("Finished creating node configuration file for node : ( id, config file ) : ( " +
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
				                             NumberOfManagersToStart ,
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

			RoundRobin.AddHost(uri);

			Logger.DebugWithLineNumber("Start: AppDomainManagerTask.StartTask");

			appDomainManagerTask.StartTask(new CancellationTokenSource());

			Logger.DebugWithLineNumber("Finished: AppDomainManagerTask.StartTask");

			friendlyname = appDomainManagerTask.GetAppDomainUniqueId();
		}
	}
}