using System;
using System.Configuration;
using System.Reflection;
using Autofac;
using log4net;
using log4net.Config;
using NodeTest.JobHandlers;
using Stardust.Node;
using Stardust.Node.API;

namespace NodeConsoleHost
{
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
}