using System;
using System.Threading;
using log4net;
using log4net.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Support.Security
{
	class Program
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));

		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += appDomainUnhandledException;

			XmlConfigurator.Configure();
			Console.WriteLine(@"Please be patient, don't close this window!");
			Console.WriteLine("");
			log.Debug("Starting Teleopti.Support.Security");
			log.Debug("Was called with args: " + string.Join(" ", args));

			try
			{
				var command = UpgradeCommand.Parse(args);
				var upgrade = new UpgradeRunner(null);
				upgrade.Upgrade(command);
				handleSuccess();
			}
			catch (Exception e)
			{
				handleError(e);
			}

			Thread.Sleep(TimeSpan.FromSeconds(3));
			Environment.ExitCode = 0;
		}

		private static void handleSuccess()
		{
			Thread.Sleep(TimeSpan.FromSeconds(3));
			log.Info("Teleopti.Support.Security successful");
			Environment.ExitCode = 0;
		}

		private static void appDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			handleError((Exception) e.ExceptionObject);
		}

		private static void handleError(Exception e)
		{
			log.Fatal("Teleopti.Support.Security has exited with fatal error:");
			log.Fatal(e.Message);
			log.Fatal(e.StackTrace);
			Thread.Sleep(TimeSpan.FromSeconds(5));
			Environment.Exit(1);
		}
	}
}