using System;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Http;
using Autofac;
using log4net;
using log4net.Config;
using Microsoft.Owin.Hosting;
using Owin;
using Stardust.Manager;
using Stardust.Manager.Helpers;
using Stardust.Manager.Models;

namespace ManagerConsoleHost
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

        private static ManagerStarter ManagerStarter { get; set; }

        public static void Main(string[] args)
        {
            var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

            SetConsoleCtrlHandler(ConsoleCtrlCheck,
                true);

            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

            WhoAmI = "[MANAGER CONSOLE HOST, " + Environment.MachineName.ToUpper() + "]";

            LogHelper.LogInfoWithLineNumber(Logger,
                WhoAmI + " : started.");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var managerConfiguration = new ManagerConfiguration
            {
                ConnectionString =
                    ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString,
                Route = ConfigurationManager.AppSettings["route"]
            };

            Uri baseAddress = new Uri(ConfigurationManager.AppSettings["baseAddress"]);

            var managerAddress = baseAddress.Scheme + "://+:" +
                                 baseAddress.Port + "/";

			var container = new ContainerBuilder().Build();
            var config = new HttpConfiguration();

            using (WebApp.Start(managerAddress,
                appBuilder =>
                {
                    appBuilder.UseAutofacMiddleware(container);
                    // Configure Web API for self-host. 
                    appBuilder.UseStardustManager(managerConfiguration, container);

                    appBuilder.UseAutofacWebApi(config);
                    appBuilder.UseWebApi(config);
                }))
            {
                LogHelper.LogInfoWithLineNumber(Logger,
                    WhoAmI + ": Started listening on port : ( " + baseAddress + " )");
           
                ManagerStarter = new ManagerStarter();
                ManagerStarter.Start(managerConfiguration, container );

                QuitEvent.WaitOne();
            }
        }

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine handler,
            bool add);

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            if (ctrlType == CtrlTypes.CtrlCloseEvent ||
                ctrlType == CtrlTypes.CtrlShutdownEvent)
            {
                LogHelper.LogDebugWithLineNumber(Logger,
                    WhoAmI + " : ConsoleCtrlCheck called.");

                QuitEvent.Set();

                return true;
            }

            return false;
        }

        private static void CurrentDomain_DomainUnload(object sender,
            EventArgs e)
        {
            LogHelper.LogDebugWithLineNumber(Logger,
                WhoAmI + " : CurrentDomain_DomainUnload called.");

            if (ManagerStarter != null)
            {
               // ManagerStarter.Stop();
            }

            QuitEvent.Set();
        }

        private static void CurrentDomain_UnhandledException(object sender,
            UnhandledExceptionEventArgs e)
        {
            if (!e.IsTerminating)
            {
                var exp = e.ExceptionObject as Exception;

                LogHelper.LogErrorWithLineNumber(Logger,
                    WhoAmI + ": Unhandled Exception",
                    exp);
            }
        }
    }
}