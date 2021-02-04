using System;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Http;
using Autofac;
using log4net;
using log4net.Config;
using Microsoft.Owin.Host.HttpListener;
using Microsoft.Owin.Hosting;
using Owin;
using Stardust.Manager;
using Stardust.Manager.Extensions;

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

		public static void Main(string[] args)
		{
			var configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configurationFile));

			SetConsoleCtrlHandler(ConsoleCtrlCheck,
			                      true);

			var managerName = ConfigurationManager.AppSettings["ManagerName"];
			var baseAddress = new Uri(ConfigurationManager.AppSettings["baseAddress"]);

			var managerAddress = $"{baseAddress.Scheme}://+:{baseAddress.Port}/";

			AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;


			WhoAmI =
                $"[MANAGER CONSOLE HOST ( {managerName}, {managerAddress} ),{Environment.MachineName.ToUpper()}]";

			Logger.InfoWithLineNumber($"{WhoAmI} : started.");

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			var managerConfiguration = new ManagerConfiguration(
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString,
				ConfigurationManager.AppSettings["route"],
				int.Parse(ConfigurationManager.AppSettings["AllowedNodeDownTimeSeconds"]),
				int.Parse(ConfigurationManager.AppSettings["CheckNewJobIntervalSeconds"]),
				int.Parse(ConfigurationManager.AppSettings["purgeJobsBatchSize"]),
				int.Parse(ConfigurationManager.AppSettings["purgeJobsIntervalHours"]),
				int.Parse(ConfigurationManager.AppSettings["PurgeJobsOlderThanHours"]),
				int.Parse(ConfigurationManager.AppSettings["PurgeNodesIntervalHours"]));

            var config = new HttpConfiguration();

			using (WebApp.Start(managerAddress,
			                    appBuilder =>
			                    {
				                    string owinListenerName = "Microsoft.Owin.Host.HttpListener.OwinHttpListener";
				                    OwinHttpListener owinListener = (OwinHttpListener) appBuilder.Properties[owinListenerName];

                                    owinListener.GetRequestProcessingLimits(out _, out _);

				                    owinListener.SetRequestQueueLimit(int.MaxValue);
				                    owinListener.SetRequestProcessingLimits(int.MaxValue, int.MaxValue);
									
				                    // Configure Web API for self-host. 
				                    appBuilder.UseStardustManager(managerConfiguration);

				                    appBuilder.UseAutofacWebApi(config);
				                    appBuilder.UseWebApi(config);

			                    }))
			{
				Logger.InfoWithLineNumber($"{WhoAmI}: Started listening on port : ( {baseAddress} )");

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
				Logger.DebugWithLineNumber(WhoAmI + " : ConsoleCtrlCheck called.");

				QuitEvent.Set();

				return true;
			}

			return false;
		}

		private static void CurrentDomain_DomainUnload(object sender,
		                                               EventArgs e)
		{
			Logger.DebugWithLineNumber(WhoAmI + " : CurrentDomain_DomainUnload called.");

			QuitEvent.Set();
		}

		private static void CurrentDomain_UnhandledException(object sender,
		                                                     UnhandledExceptionEventArgs e)
		{
			var exp = e.ExceptionObject as Exception;

			if (exp == null) return;

			Logger.FatalWithLineNumber(exp.Message,exp);
			//should crash integration tests
			throw exp;
		}
	}
}