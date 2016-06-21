using System;
using System.Threading;
using log4net;
using log4net.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

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
				var commandLineArgument = new CommandLineArgument(args);
				var arguments = commandLineArgument.GetDatabaseArguments();
				if (arguments.CheckTenantConnectionStrings)
				{
					var tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(arguments.TenantStoreConnectionString);
					var checker = new CheckTenantConnectionStrings(tenantUnitOfWorkManager, tenantUnitOfWorkManager);
					checker.CheckConnectionStrings(arguments.TenantStoreConnectionString);
					log.Debug("Teleopti.Support.Security successful");
					return;
				}
				var upgrade = new UpgradeRunner();
				upgrade.Upgrade(arguments);
			}
			catch (Exception e)
			{
				handleError(e);
			}

			Thread.Sleep(TimeSpan.FromSeconds(3));
			Environment.ExitCode = 0;
		}

		private static void appDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			handleError((Exception)e.ExceptionObject);
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
