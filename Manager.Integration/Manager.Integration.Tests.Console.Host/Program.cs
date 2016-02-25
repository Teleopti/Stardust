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
				LogHelper.LogInfoWithLineNumber(Logger,
				                                "Started listening on port : ( " + address + " )");

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

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Start.");

			SetConsoleCtrlHandler(ConsoleCtrlCheck,
			                      true);

			//---------------------------------------------------------
			// Number of managers and number of nodes to start up.
			//---------------------------------------------------------
			NumberOfManagersToStart = Settings.Default.NumberOfManagersToStart;
			NumberOfNodesToStart = Settings.Default.NumberOfNodesToStart;

			if (args.Any())
			{
				LogHelper.LogDebugWithLineNumber(Logger,
				                                 "Has command arguments.");

				foreach (var s in args)
				{
					var values = s.Split('=');

					// Managers.
					if (values[0].Equals(ManagersCommandArgument, StringComparison.InvariantCultureIgnoreCase))
					{
						NumberOfManagersToStart = Convert.ToInt32(values[1]);
					}

					// Nodes.
					if (values[0].Equals(NodesCommandArgument, StringComparison.InvariantCultureIgnoreCase))
					{
						NumberOfNodesToStart = Convert.ToInt32(values[1]);
					}
				}
			}

			LogHelper.LogInfoWithLineNumber(Logger,
			                                NumberOfManagersToStart + " number of managers will be started.");

			LogHelper.LogInfoWithLineNumber(Logger,
			                                NumberOfNodesToStart + " number of nodes will be started.");


			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "AppDomain.CurrentDomain.DomainUnload");
			AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "AppDomain.CurrentDomain.UnhandledException");
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			DirectoryManagerAssemblyLocationFullPath =
				new DirectoryInfo(Path.Combine(Settings.Default.ManagerAssemblyLocationFullPath,
				                               _buildMode));

			DirectoryManagerConfigurationFileFullPath =
				new DirectoryInfo(Path.Combine(Settings.Default.ManagerConfigurationFileFullPath,
				                               _buildMode));

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "DirectoryManagerConfigurationFileFullPath : " +
			                                 DirectoryManagerConfigurationFileFullPath.FullName);


			ManagerConfigurationFile =
				new FileInfo(Path.Combine(DirectoryManagerConfigurationFileFullPath.FullName,
				                          Settings.Default.ManagerConfigurationFileName));

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "ManagerConfigurationFile : " + ManagerConfigurationFile.FullName);

			CopiedManagerConfigurationFiles = new Dictionary<Uri, FileInfo>();

			for (var i = 0; i < NumberOfManagersToStart; i++)
			{
				var portNumber = Settings.Default.ManagerEndpointPortNumberStart + i;

				Uri uri;

				var copiedManagerConfigurationFile =
					CopyManagerConfigurationFile(ManagerConfigurationFile, i + 1, portNumber, out uri);

				CopiedManagerConfigurationFiles.Add(uri, copiedManagerConfigurationFile);
			}

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Created " + CopiedManagerConfigurationFiles.Count + " manager configuration files.");


			DirectoryNodeConfigurationFileFullPath =
				new DirectoryInfo(Path.Combine(Settings.Default.NodeConfigurationFileFullPath,
				                               _buildMode));

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "DirectoryNodeConfigurationFileFullPath : " +
			                                 DirectoryNodeConfigurationFileFullPath.FullName);

			NodeConfigurationFile =
				new FileInfo(Path.Combine(DirectoryNodeConfigurationFileFullPath.FullName,
				                          Settings.Default.NodeConfigurationFileName));

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "NodeConfigurationFile : " + NodeConfigurationFile.FullName);

			NodeconfigurationFiles = new Dictionary<string, FileInfo>();

			PortStartNumber =
				Settings.Default.NodeEndpointPortNumberStart;

			if (NumberOfNodesToStart > 0)
			{
				for (var i = 1; i <= NumberOfNodesToStart; i++)
				{
					LogHelper.LogDebugWithLineNumber(Logger,
					                                 "Start creating node configuration file for node id : " + i);

					var nodeConfig = CreateNodeConfigurationFile(i);

					LogHelper.LogDebugWithLineNumber(Logger,
					                                 "Finished creating node configuration file for node : ( id, config file ) : ( " +
					                                 i + ", " + nodeConfig.FullName + " )");
				}
			}

			//-------------------------------------------------------
			// App domain manager tasks.
			//-------------------------------------------------------
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "AppDomainManagerTasks");

			AppDomainManagerTasks = new List<AppDomainManagerTask>();

			foreach (var copiedManagerConfigurationFile in CopiedManagerConfigurationFiles.Values)
			{
				var appDomainManagerTask =
					new AppDomainManagerTask(_buildMode,
					                         DirectoryManagerAssemblyLocationFullPath,
					                         copiedManagerConfigurationFile,
					                         Settings.Default.ManagerAssemblyName);

				AppDomainManagerTasks.Add(appDomainManagerTask);

				LogHelper.LogDebugWithLineNumber(Logger,
				                                 "Start: AppDomainManagerTask.StartTask");

				appDomainManagerTask.StartTask(new CancellationTokenSource());
			}

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Finished: AppDomainManagerTask.StartTask");

			//-------------------------------------------------------
			//-------------------------------------------------------
			DirectoryNodeAssemblyLocationFullPath =
				new DirectoryInfo(Path.Combine(Settings.Default.NodeAssemblyLocationFullPath,
				                               _buildMode));

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "DirectoryNodeAssemblyLocationFullPath : " +
			                                 DirectoryNodeAssemblyLocationFullPath.FullName);

			AppDomainNodeTasks = new List<AppDomainNodeTask>();

			foreach (var nodeconfigurationFile in NodeconfigurationFiles)
			{
				LogHelper.LogDebugWithLineNumber(Logger,
				                                 "AppDomainNodeTask");

				var appDomainNodeTask =
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

		public static List<AppDomainManagerTask> AppDomainManagerTasks { get; set; }

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

			var copiedConfigurationFile =
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
			var exp = e.ExceptionObject as Exception;

			if (exp != null)
			{
				LogHelper.LogFatalWithLineNumber(Logger,
				                                 exp.Message,
				                                 exp);
			}
		}

		private static void CurrentDomainOnDomainUnload(object sender,
		                                                EventArgs eventArgs)
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Start CurrentDomainOnDomainUnload.");

			//-------------------------------------------
			// Shut down nodes.
			//-------------------------------------------
			foreach (var appDomainNodeTask in AppDomainNodeTasks)
			{
				appDomainNodeTask.Dispose();
			}

			//-------------------------------------------
			// Shut down managers.
			//-------------------------------------------
			foreach (var appDomainManagerTask in AppDomainManagerTasks)
			{
				appDomainManagerTask.Dispose();
			}

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
		                                                    int i,
		                                                    int portNumber,
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

			config.AppSettings.Settings["BaseAddress"].Value = uri.ToString();
			config.Save();

			return copyManagerConfigurationFile;
		}

		public static bool ShutDownManager(string friendlyName)
		{
			var ret = false;

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Started.");

			if (AppDomainManagerTasks != null && AppDomainManagerTasks.Any())
			{
				AppDomainManagerTask managerToDispose = null;

				foreach (var appDomainManagerTask in AppDomainManagerTasks)
				{
					if (appDomainManagerTask.GetAppDomainFriendlyName()
						.Equals(friendlyName,
						        StringComparison.InvariantCultureIgnoreCase))
					{
						managerToDispose = appDomainManagerTask;

						break;
					}
				}

				if (managerToDispose != null)
				{
					LogHelper.LogDebugWithLineNumber(Logger,
					                                 "Start to manager (appdomain) with friendly name :" + friendlyName);

					managerToDispose.Dispose();

					AppDomainManagerTasks.Remove(managerToDispose);

					LogHelper.LogDebugWithLineNumber(Logger,
					                                 "Finished to dispose manager (appdomain) with friendly name :" + friendlyName);

					ret = true;
				}
			}

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Finished.");

			return ret;
		}

		public static bool ShutDownNode(string friendlyName)
		{
			var ret = false;

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Started.");

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

					ret = true;
				}
			}

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Finished.");

			return ret;
		}

		public static void StartNewNode(out string friendlyName)
		{
			friendlyName = null;

			NumberOfNodesToStart++;

			if (NumberOfNodesToStart > 0)
			{
				LogHelper.LogDebugWithLineNumber(Logger,
				                                 "Start creating node configuration file for node id : " + NumberOfNodesToStart);

				var nodeConfig = CreateNodeConfigurationFile(NumberOfNodesToStart);

				friendlyName = nodeConfig.Name;

				LogHelper.LogDebugWithLineNumber(Logger,
				                                 "AppDomainNodeTask");

				var appDomainNodeTask =
					new AppDomainNodeTask(_buildMode,
					                      DirectoryNodeAssemblyLocationFullPath,
					                      nodeConfig,
					                      Settings.Default.NodeAssemblyName);

				LogHelper.LogDebugWithLineNumber(Logger,
				                                 "Start : AppDomainNodeTask.StartTask");

				appDomainNodeTask.StartTask(new CancellationTokenSource());

				LogHelper.LogDebugWithLineNumber(Logger,
				                                 "Finished : AppDomainNodeTask.StartTask");

				AppDomainNodeTasks.Add(appDomainNodeTask);

				LogHelper.LogDebugWithLineNumber(Logger,
				                                 "Finished creating node configuration file for node : ( id, config file ) : ( " +
				                                 NumberOfNodesToStart + ", " + nodeConfig.FullName + " )");
			}
		}

		public static List<string> GetAllmanagers()
		{
			var listToReturn = new List<string>();

			if (AppDomainManagerTasks != null)
			{
				foreach (var appDomainManagerTask in AppDomainManagerTasks)
				{
					listToReturn.Add(appDomainManagerTask.GetAppDomainFriendlyName());
				}
			}

			return listToReturn;
		}

		public static List<string> GetAllNodes()
		{
			var listToReturn = new List<string>();

			if (AppDomainNodeTasks != null)
			{
				foreach (var appDomainNodeTask in AppDomainNodeTasks)
				{
					listToReturn.Add(appDomainNodeTask.GetAppDomainFriendlyName());
				}
			}

			return listToReturn;
		}

		public static void StartNewManager(out string friendlyname)
		{
			NumberOfManagersToStart++;

			var portNumber = Settings.Default.ManagerEndpointPortNumberStart + (NumberOfManagersToStart - 1);

			Uri uri;

			var copiedManagerConfigurationFile =
				CopyManagerConfigurationFile(ManagerConfigurationFile,
				                             NumberOfManagersToStart + 1,
				                             portNumber,
				                             out uri);

			CopiedManagerConfigurationFiles.Add(uri, copiedManagerConfigurationFile);

			var appDomainManagerTask =
				new AppDomainManagerTask(_buildMode,
				                         DirectoryManagerAssemblyLocationFullPath,
				                         copiedManagerConfigurationFile,
				                         Settings.Default.ManagerAssemblyName);

			AppDomainManagerTasks.Add(appDomainManagerTask);

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Start: AppDomainManagerTask.StartTask");

			appDomainManagerTask.StartTask(new CancellationTokenSource());

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Finished: AppDomainManagerTask.StartTask");

			friendlyname = appDomainManagerTask.GetAppDomainFriendlyName();
		}
	}
}