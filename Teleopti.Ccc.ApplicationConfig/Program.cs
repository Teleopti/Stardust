using System;
using System.Globalization;
using log4net;
using log4net.Config;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.ApplicationConfig.Common;
using Teleopti.Ccc.DBConverter;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig
{
	class Program
	{
		private static DatabaseConvert _databaseConvert;
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

			if (argument.DefaultResolution != 0)
			{
				Console.Write("Interval resolution change is a cumbersome operation, are you sure you want to continue (Y/N) ?");
				string input = Console.ReadLine();
				if (input.ToLower(CultureInfo.CurrentCulture) != "y")
				{
					Console.WriteLine("Operation aborted...");
					return;
				}
			}

			if (argument.OnlyRunMergeDefaultResolution)
			{
				loadSessionAndLogOn(argument);
				_databaseConvert.MergeToResolution(argument.FromDate, argument.ToDate, argument.TimeZone, argument.DefaultResolution);
			}
			else
			{
				//Create default aggregate roots
				var programHelper = new ProgramHelper();

				//create new "empty"
				var saveBusinessUnitAction = new Action(() => programHelper.GetDefaultAggregateRoot(argument));
				programHelper.CreateNewEmptyCcc7(saveBusinessUnitAction);
			}
		}

		static void currentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = (Exception)e.ExceptionObject;
			logger.Error(ex.Message);
		}

		private static void loadSessionAndLogOn(ICommandLineArgument argument)
		{
			var databaseHandler = new DatabaseHandler(argument);

			IBusinessUnit businessUnit;
			using (ISession session = databaseHandler.SessionFactory.OpenSession())
			{
				ICriteria criteria =
					session.CreateCriteria(typeof (BusinessUnit)).Add(Restrictions.Eq("Description.Name", argument.BusinessUnit));
				businessUnit = criteria.UniqueResult<IBusinessUnit>();
				LazyLoadingManager.Initialize(businessUnit.UpdatedBy);
			}
			var programHelper = new ProgramHelper();
			programHelper.LogOn(argument, databaseHandler, businessUnit);
		}
	}
}
