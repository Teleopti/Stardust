using System;
using System.Configuration;
using System.Reflection;
using Autofac;
using Core.Stardust.Node;
using log4net;
using NodeTest.JobHandlers;
using Stardust.Node;
using Stardust.Node.Extensions;


namespace Core.Node.ConsoleHost
{
    public class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (Program));

		private static string WhoAmI { get; set; }

		private static NodeStarter NodeStarter { get; set; }

		public static IContainer Container { get; set; }

		public static void Main()
		{
            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                NodeStarter?.Stop();
            };

            //var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            //XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var aaa = ConfigurationManager.AppSettings["ManagerLocation"];

            var nodeConfig = new NodeConfiguration(
                new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
                Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
                int.Parse(ConfigurationManager.AppSettings["Port"]),
                ConfigurationManager.AppSettings["NodeName"],
                int.Parse(ConfigurationManager.AppSettings["PingToManagerSeconds"]),
                int.Parse(ConfigurationManager.AppSettings["SendJobDetailToManagerMilliSeconds"]),
                bool.Parse(ConfigurationManager.AppSettings["EnableGc"]
                ));

            //var nodeConfig = new NodeConfiguration(
            //    new Uri("http://localhost:9001/StardustDashboard/"), 
            //   typeof(WorkerModule).Assembly,
            //   14100 , 
            //   "Node1",
            //   20,2000
            //   ,true );


			WhoAmI = $"[NODE CONSOLE HOST ( {nodeConfig.NodeName}, {nodeConfig.BaseAddress} ), {Environment.MachineName.ToUpper()}]";
			Logger.InfoWithLineNumber($"{WhoAmI} : started.");


            var nodeConfigurationService = new NodeConfigurationService();
            nodeConfigurationService.AddConfiguration(nodeConfig.BaseAddress.Port, nodeConfig);

            var builder = new ContainerBuilder();
            builder.RegisterInstance(nodeConfigurationService);
			builder.RegisterModule(new WorkerModule());
			builder.RegisterModule<NodeModule>();
			Container = builder.Build();

			NodeStarter = new NodeStarter(Container);


            _ = NodeStarter.Start(nodeConfig);

        }

		private static void CurrentDomain_DomainUnload(object sender,
		                                               EventArgs e)
		{
            Logger.InfoWithLineNumber($"{WhoAmI} : domain unloaded.");
            NodeStarter?.Stop();
		}

		private static void CurrentDomain_UnhandledException(object sender,
		                                                     UnhandledExceptionEventArgs e)
		{
            if (e.ExceptionObject is Exception exp)
			{
				Logger.FatalWithLineNumber(exp.Message,exp);
				//Should crash integration tests
				throw exp;
			}
		}
    }
}
