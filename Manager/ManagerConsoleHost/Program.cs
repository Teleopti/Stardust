using System;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using log4net.Config;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Owin;
using Stardust.Manager;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerConsoleHost
{
    public class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (Program));
         
        private static string WhoAmI { get; set; }

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
                BaseAddress = new Uri(ConfigurationManager.AppSettings["baseAddress"]),
                ConnectionString =
                    ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString
            };

            

            string managerAddress = managerConfiguration.BaseAddress.Scheme + "://+:" +
                                    managerConfiguration.BaseAddress.Port + "/";

            using (WebApp.Start(managerAddress,
                appBuilder =>
                {
                    var builder = new ContainerBuilder();

                    builder.RegisterType<NodeManager>()
                        .As<INodeManager>();

                    builder.RegisterType<JobManager>();

                    builder.RegisterType<HttpSender>()
                        .As<IHttpSender>();

                    builder.Register(
                        c => new JobRepository(managerConfiguration.ConnectionString))
                        .As<IJobRepository>();

                    builder.Register(
                        c => new WorkerNodeRepository(managerConfiguration.ConnectionString))
                        .As<IWorkerNodeRepository>();

                    builder.RegisterApiControllers(typeof (ManagerController).Assembly);

                    builder.RegisterInstance(managerConfiguration);

                    var container = builder.Build();

                    // Configure Web API for self-host. 
                    appBuilder.UseStardustManager(container, ConfigurationManager.AppSettings["routeName"]);

                    appBuilder.UseDefaultFiles(new DefaultFilesOptions
                    {
                        FileSystem = new PhysicalFileSystem(@".\StardustDashboard"),
                        RequestPath = new PathString("/StardustDashboard")
                    });

                    appBuilder.UseStaticFiles();

                }))
            {
                LogHelper.LogInfoWithLineNumber(Logger,
                    WhoAmI + ": Started listening on port : ( " + managerConfiguration.BaseAddress + " )");

                ManagerStarter = new ManagerStarter();
                ManagerStarter.Start(managerConfiguration);

                QuitEvent.WaitOne();
            }
        }

        private static ManagerStarter ManagerStarter { get; set; }

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
                LogHelper.LogInfoWithLineNumber(Logger,
                                                WhoAmI + " : ConsoleCtrlCheck called.");

                if (ManagerStarter != null)
                {
                    ManagerStarter.Stop();
                }

                QuitEvent.Set();

                return true;
            }

            return false;
        }

        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

        private static void CurrentDomain_DomainUnload(object sender,
                                                       EventArgs e)
        {
            LogHelper.LogInfoWithLineNumber(Logger,
                                            WhoAmI + " : CurrentDomain_DomainUnload called.");

            if (ManagerStarter != null)
            {
                ManagerStarter.Stop();
            }

            QuitEvent.Set();
        }

        private static void CurrentDomain_UnhandledException(object sender,
                                                             UnhandledExceptionEventArgs e)
        {
            if (!e.IsTerminating)
            {
                Exception exp = e.ExceptionObject as Exception;

                LogHelper.LogErrorWithLineNumber(Logger,
                                                 WhoAmI + ": Unhandled Exception",
                                                 exp);
            }
        }
    }
}