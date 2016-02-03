using System;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Threading;
using log4net;
using log4net.Config;
using Stardust.Manager;
using Stardust.Manager.Models;

namespace ManagerConsoleHost
{
    internal class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (ManagerController));

        private static string WhoAmI { get; set; }

        private static void Main(string[] args)
        {
            SetConsoleCtrlHandler(ConsoleCtrlCheck,
                                  true);

            System.Console.CancelKeyPress += ConsoleOnCancelKeyPress; 

            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

            WhoAmI = "[MANAGER, " + Environment.MachineName.ToUpper() + "]";

            XmlConfigurator.Configure();

            Logger.Info(WhoAmI + " : started.");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var config = new ManagerConfiguration
            {
                BaseAdress = ConfigurationManager.AppSettings["baseAddress"],
                ConnectionString =
                    ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString
            };

            _managerStarter = new ManagerStarter();

            _managerStarter.Start(config);

            QuitEvent.WaitOne();            
        }

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
                _managerStarter.Stop();

                QuitEvent.Set();

                return true;
            }

            return false;
        }


        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

        private static void ConsoleOnCancelKeyPress(object sender,
                                                    ConsoleCancelEventArgs e)
        {
            _managerStarter.Stop();

            QuitEvent.Set();

            e.Cancel = true;
        }


        private static ManagerStarter _managerStarter;

        private static void CurrentDomain_DomainUnload(object sender,
                                                       EventArgs e)
        {
            Console.WriteLine("Manager console host CurrentDomain_DomainUnload");

            _managerStarter.Stop();
        }

        private static void CurrentDomain_UnhandledException(object sender,
                                                             UnhandledExceptionEventArgs e)
        {
            Logger.Error(WhoAmI + ": Unhandeled Exception in ManagerConsoleHost");
        }
    }
}