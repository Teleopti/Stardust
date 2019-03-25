using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Autofac;
using log4net;
using log4net.Config;
using NodeTest.JobHandlers;
using Stardust.Node;
using Stardust.Node.Extensions;

namespace NodeConsoleHost
{
	public class Program
	{
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

		private static readonly ILog Logger = LogManager.GetLogger(typeof (Program));

		private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

		private static string WhoAmI { get; set; }

		private static NodeStarter NodeStarter { get; set; }

		public static IContainer Container { get; set; }


		[DllImport("Kernel32")]
		public static extern bool SetConsoleCtrlHandler(HandlerRoutine handler,
		                                                bool add);

		private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
		{
			if (ctrlType == CtrlTypes.CtrlCloseEvent ||
			    ctrlType == CtrlTypes.CtrlShutdownEvent)
			{
				NodeStarter.Stop();

				QuitEvent.Set();

				return true;
			}

			return false;
		}

		public static void Main()
		{
			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

			SetConsoleCtrlHandler(ConsoleCtrlCheck,
			                      true);
			
			AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			var enableGc = bool.Parse(ConfigurationManager.AppSettings["EnableGc"]);
			var nodeConfig = new NodeConfiguration(
				new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
				Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
				int.Parse(ConfigurationManager.AppSettings["Port"]),
				ConfigurationManager.AppSettings["NodeName"],
				int.Parse(ConfigurationManager.AppSettings["PingToManagerSeconds"]),
				int.Parse(ConfigurationManager.AppSettings["SendJobDetailToManagerMilliSeconds"]),enableGc
				);

			WhoAmI = "[NODE CONSOLE HOST ( " + nodeConfig.NodeName + ", " + nodeConfig.BaseAddress + " ), " + Environment.MachineName.ToUpper() + "]";
			Logger.InfoWithLineNumber(WhoAmI + " : started.");


            var nodeConfigurationService = new NodeConfigurationService();
            nodeConfigurationService.AddConfiguration(nodeConfig.BaseAddress.Port ,nodeConfig);

            var builder = new ContainerBuilder();
            builder.RegisterInstance(nodeConfigurationService);
			builder.RegisterModule(new WorkerModule());
			builder.RegisterModule<NodeModule>();
			Container = builder.Build();

			NodeStarter = new NodeStarter();
			
			NodeStarter.Start(nodeConfig,
			                  Container);

			QuitEvent.WaitOne();
		}

		private static void CurrentDomain_DomainUnload(object sender,
		                                               EventArgs e)
		{
			if (NodeStarter != null)
			{
				NodeStarter.Stop();
			}

			QuitEvent.Set();
		}

		private static void CurrentDomain_UnhandledException(object sender,
		                                                     UnhandledExceptionEventArgs e)
		{
			var exp = e.ExceptionObject as Exception;

			if (exp != null)
			{
				Logger.FatalWithLineNumber(exp.Message,exp);
				//Should crash integration tests
				throw exp;
			}
		}
	}
}