using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using NodeTest.JobHandlers;
using Stardust.Core.Node;

namespace CoreNodeConsoleHost
{
    public class Program
    {
        private static string WhoAmI { get; set; }
        public static IContainer Container { get; set; }
        private static NodeStarter NodeStarter { get; set; }

        public static ContainerBuilder ContainerBuilder;

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            var managerLocation = configuration["ManagerLocation"];

            var loadedAssembly = Assembly.Load(configuration["HandlerAssembly"]);
            var nodeConfig = new NodeConfiguration(
                new Uri(configuration["ManagerLocation"]),
                loadedAssembly,
                int.Parse(configuration["Port"]),
                configuration["NodeName"],
                int.Parse(configuration["PingToManagerSeconds"]),
                int.Parse(configuration["SendJobDetailToManagerMilliSeconds"]),
                bool.Parse(configuration["EnableGc"])
                );

            WhoAmI = "[NODE CONSOLE HOST ( " + nodeConfig.NodeName + ", " + nodeConfig.BaseAddress + " ), " + Environment.MachineName.ToUpper() + "]";

            var nodeConfigurationService = new NodeConfigurationService();
            nodeConfigurationService.AddConfiguration(nodeConfig.BaseAddress.Port, nodeConfig);

            //var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            //XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

            //SetConsoleCtrlHandler(ConsoleCtrlCheck,
            //                      true);

            //AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //var enableGc = bool.Parse(ConfigurationManager.AppSettings["EnableGc"]);
            //var nodeConfig = new NodeConfiguration(
            //    new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
            //    Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
            //    int.Parse(ConfigurationManager.AppSettings["Port"]),
            //    ConfigurationManager.AppSettings["NodeName"],
            //    int.Parse(ConfigurationManager.AppSettings["PingToManagerSeconds"]),
            //    int.Parse(ConfigurationManager.AppSettings["SendJobDetailToManagerMilliSeconds"]), enableGc
            //    );

            //WhoAmI = "[NODE CONSOLE HOST ( " + nodeConfig.NodeName + ", " + nodeConfig.BaseAddress + " ), " + Environment.MachineName.ToUpper() + "]";
            //Logger.InfoWithLineNumber(WhoAmI + " : started.");


            //var nodeConfigurationService = new NodeConfigurationService();
            //nodeConfigurationService.AddConfiguration(nodeConfig.BaseAddress.Port, nodeConfig);

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(nodeConfigurationService);
            containerBuilder.RegisterModule(new WorkerModule());
            //containerBuilder.RegisterModule<NodeModule>();
            //ContainerBuilder = containerBuilder;
            //containerBuilder.Register<>().As<IServiceProvider>()
            //Container = containerBuilder.Build();
            //var provider = new AutofacServiceProvider(Container);
            //Container.ComponentRegistry.Register();
            NodeStarter = new NodeStarter();
            
            NodeStarter.Start(nodeConfig, containerBuilder, new WorkerModule());

            //QuitEvent.WaitOne();
        }

        private static void CurrentDomain_DomainUnload(object sender,
                                                       EventArgs e)
        {
            //if (NodeStarter != null)
            //{
            //    NodeStarter.Stop();
            //}

            //QuitEvent.Set();
        }

        private static void CurrentDomain_UnhandledException(object sender,
                                                             UnhandledExceptionEventArgs e)
        {
            //var exp = e.ExceptionObject as Exception;

            //if (exp != null)
            //{
            //    Logger.FatalWithLineNumber(exp.Message, exp);
            //    //Should crash integration tests
            //    throw exp;
            //}
        }
    }
}
