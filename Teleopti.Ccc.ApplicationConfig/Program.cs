using System;
using log4net;
using log4net.Config;
using Teleopti.Ccc.ApplicationConfig.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig
{
	class Program
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Program));

		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += currentDomainUnhandledException;
			XmlConfigurator.Configure();

			Console.WriteLine(ProgramHelper.VersionInfo);
			if (args.Length < 1)
			{
				Console.WriteLine(ProgramHelper.UsageInfo);
				return;
			}

			logger.Info("Starting CccAppConfig");

			ICommandLineArgument argument = new CommandLineArgument(args);

			//Create default aggregate roots
			var programHelper = new ProgramHelper();

			//create new "empty"
			var saveBusinessUnitAction = new Action(() => programHelper.GetDefaultAggregateRoot(argument));
			programHelper.CreateNewEmptyCcc7(saveBusinessUnitAction);
		}

		static void currentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = (Exception)e.ExceptionObject;
			logger.Error(ex.Message);
		}
	}
}
