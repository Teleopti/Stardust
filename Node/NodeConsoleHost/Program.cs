using System;
using System.Configuration;
using System.Reflection;
using System.Runtime.CompilerServices;
using Autofac;
using Autofac.Core;
using log4net;
using log4net.Config;
using Stardust.Node;
using Stardust.Node.API;
using Stardust.Node.Interfaces;

namespace NodeConsoleHost
{
	internal class Program
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

		private static void Main(string[] args)
		{
			XmlConfigurator.Configure();

            Logger.Info("NodeConsoleHost: started.");

            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			var nodeConfig = new NodeConfiguration(new Uri(ConfigurationManager.AppSettings["BaseAddress"]),
																new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
																Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
																ConfigurationManager.AppSettings["NodeName"]);
			Container = new ContainerBuilder().Build();

            _nodeStarter = new NodeStarter();

            _nodeStarter.Start(nodeConfig, Container);            
        }

	    private static INodeStarter _nodeStarter;

	    public static IContainer Container { get; set; }

	    private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            _nodeStarter.Stop();
        }

        private static void CurrentDomain_UnhandledException(object sender,
																			  UnhandledExceptionEventArgs e)
		{
			Logger.Error("Unhandeled Exception in NodeConsoleHost");
		}
	}
}