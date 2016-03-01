﻿using System;
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
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;

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

		private static INodeStarter NodeStarter { get; set; }

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

			var nodeName = ConfigurationManager.AppSettings["NodeName"];

			WhoAmI = "[NODE CONSOLE HOST ( " + nodeName + " ), " + Environment.MachineName.ToUpper() + "]";

			LogHelper.LogInfoWithLineNumber(Logger,
			                                WhoAmI + " : started.");

			AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			var nodeConfig = new NodeConfiguration(new Uri(ConfigurationManager.AppSettings["BaseAddress"]),
			                                       new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
			                                       Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
			                                       ConfigurationManager.AppSettings["NodeName"],
												   pingToManagerIdleDelaySeconds: new TimeSpan(0,0,0, Convert.ToInt32(ConfigurationManager.AppSettings["PingToManagerIdleDelaySeconds"])).TotalMilliseconds,
												   pingToManagerRunningDelaySeconds: new TimeSpan(0, 0, 0, Convert.ToInt32(ConfigurationManager.AppSettings["PingToManagerRunningDelaySeconds"])).TotalMilliseconds);

			var builder = new ContainerBuilder();
			builder.RegisterModule(new WorkerModule());
			Container = builder.Build();

			NodeStarter = new NodeStarter();

			NodeStarter.Start(nodeConfig,
			                  Container);

			QuitEvent.WaitOne();
		}

		private static void CurrentDomain_DomainUnload(object sender,
		                                               EventArgs e)
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 WhoAmI + " : CurrentDomain_DomainUnload called.");

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
				LogHelper.LogFatalWithLineNumber(Logger,
				                                 exp.Message,
				                                 exp);
			}
		}
	}
}