using System;
using System.Globalization;
using log4net;
using log4net.Config;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.ApplicationConfig.Common;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.ApplicationConfig
{
	class Program
	{
		private static DBConverter.DatabaseConvert _databaseConvert;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			XmlConfigurator.Configure();

			Console.WriteLine(ProgramHelper.VersionInfo);
			if (args.Length < 1)
			{
				Console.WriteLine(ProgramHelper.UsageInfo);
				return;
			}

			Logger.Info("Starting CccAppConfig");

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

			if (argument.ConvertMode) //Hmm. this check is done twice, but this was added after design was implemented
			{
				//We need to check raptor compability before we create the default aggregate roots
				_databaseConvert = new DBConverter.DatabaseConvert(argument);
				ProgramHelper.CheckRaptorCompatibility();
			}
			
			if (argument.OnlyRunMergeDefaultResolution)
			{
				LoadSessionAndLogOn(argument);
				_databaseConvert.MergeToResolution(argument.FromDate, argument.ToDate, argument.TimeZone, argument.DefaultResolution);
			}
			else
			{
				//Create default aggregate roots
                ProgramHelper programHelper = new ProgramHelper();
				DefaultAggregateRoot defaultAggregateRoot = programHelper.GetDefaultAggregateRoot(argument);

				if (argument.ConvertMode)
				{
					//start the converter
					convertCCC6(argument, defaultAggregateRoot);
				}
				else
				{
					//create new "empty"
					programHelper.CreateNewEmptyCcc7(defaultAggregateRoot);
				}
			}
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = (Exception)e.ExceptionObject;
			Logger.Error(ex.Message);
		}

		private static void LoadSessionAndLogOn(ICommandLineArgument argument)
		{
			DatabaseHandler databaseHandler = new DatabaseHandler(argument);

			IBusinessUnit businessUnit;
			using (ISession session = databaseHandler.SessionFactory.OpenSession())
			{
				ICriteria criteria =
					session.CreateCriteria(typeof(BusinessUnit)).Add(Restrictions.Eq("Description.Name",
																					  argument.BusinessUnit));
				businessUnit = criteria.UniqueResult<IBusinessUnit>();
				LazyLoadingManager.Initialize(businessUnit.UpdatedBy);
			}
		    var programHelper = new ProgramHelper();
		    programHelper.LogOn(argument, databaseHandler, businessUnit);
		}

		private static void convertCCC6(ICommandLineArgument argument, DefaultAggregateRoot defaultAggregateRoot)
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				//This path ends up on the root where the dll's are stored /Peter /David
				new RtaStateGroupRepository(uow).AddRange(new RtaStateGroupCreator(@"RtaStates.xml").RtaGroupCollection);
				uow.PersistAll();
			}

			//Call the converter
			_databaseConvert.StartConverter(argument.FromDate, argument.ToDate,
											 argument.TimeZone, defaultAggregateRoot, argument.DefaultResolution);
		}
	}
}
