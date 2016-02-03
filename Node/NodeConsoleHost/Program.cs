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
using Stardust.Node.API;
using Stardust.Node.Interfaces;

namespace NodeConsoleHost
{
<<<<<<< HEAD
    internal class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (Program));

        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);


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
                _nodeStarter.Stop();

                QuitEvent.Set();

                return true;
            }

            return false;
        }


        private static void Main(string[] args)
        {
            var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

            SetConsoleCtrlHandler(ConsoleCtrlCheck,
                                  true);

            System.Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            WhoAmI = "[NODE CONSOLE HOST, " + Environment.MachineName.ToUpper() + "]";

            

            Logger.Info(WhoAmI + " : started.");

            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var nodeConfig = new NodeConfiguration(new Uri(ConfigurationManager.AppSettings["BaseAddress"]),
                                                   new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
                                                   Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
                                                   ConfigurationManager.AppSettings["NodeName"]);
            Container = new ContainerBuilder().Build();

            _nodeStarter = new NodeStarter();

            _nodeStarter.Start(nodeConfig,
                               Container);

            QuitEvent.WaitOne();
        }

        private static string WhoAmI { get; set; }

        private static void ConsoleOnCancelKeyPress(object sender,
                                                    ConsoleCancelEventArgs e)
        {
            Logger.Info(WhoAmI + " : ConsoleOnCancelKeyPress called.");

            _nodeStarter.Stop();

            QuitEvent.Set();

            e.Cancel = true;
        }

        private static INodeStarter _nodeStarter;

        public static IContainer Container { get; set; }

        private static void CurrentDomain_DomainUnload(object sender,
                                                       EventArgs e)
        {
            Logger.Info(WhoAmI + " : CurrentDomain_DomainUnload called.");

            _nodeStarter.Stop();

            QuitEvent.Set();
        }

        private static void CurrentDomain_UnhandledException(object sender,
                                                             UnhandledExceptionEventArgs e)
        {
            Logger.Error(WhoAmI + " : CurrentDomain_UnhandledException called.");
        }
    }
=======
	internal class Program
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

		private static void Main(string[] args)
		{
			XmlConfigurator.Configure();

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			var nodeConfig = new NodeConfiguration(new Uri(ConfigurationManager.AppSettings["BaseAddress"]),
																new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
																Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
																ConfigurationManager.AppSettings["NodeName"]);
			var builder = new ContainerBuilder();
			builder.RegisterModule(new WorkerModule());
			var container = builder.Build();

			new NodeStarter().Start(nodeConfig, container);
		}

		private static void CurrentDomain_UnhandledException(object sender,
																			  UnhandledExceptionEventArgs e)
		{
			Logger.Error("Unhandeled Exception in NodeConsoleHost");
		}
	}
>>>>>>> 1ab870e7a67bd026018c120f0bfdf84d0e447e49
}