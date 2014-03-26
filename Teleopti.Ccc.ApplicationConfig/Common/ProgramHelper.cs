using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Obfuscated.Security;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.ApplicationConfig.Common
{
	/// <summary>
	/// This is just a class made because we wanted to
	/// move functions from Programs Main method
	/// </summary>
	/// <remarks>
	/// Created by: peterwe
	/// Created date: 2008-11-17
	/// </remarks>
	public class ProgramHelper
	{
		public static string UsageInfo
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("***  TPS REPORT ***");
				stringBuilder.AppendLine("Usage:");
				stringBuilder.AppendLine("-SS[Source Server name]"); // Source Server Name.
				stringBuilder.AppendLine("-SD[Source Database]"); // Source Database Name.
				stringBuilder.AppendLine("-SU[Source User name]"); // Source User Name.
				stringBuilder.AppendLine("-SP[Source Password]"); // Source Password.
				stringBuilder.AppendLine("-DS[Destination Server]"); // Destination Server Name.
				stringBuilder.AppendLine("-DD[Destination Database]"); // Destination Database Name.
				stringBuilder.AppendLine("-DU[Destination User name]"); // Destination User Name.
				stringBuilder.AppendLine("-DP[Destination Password]"); // Destination Password.
				stringBuilder.AppendLine("-TZ[Time Zone] eg. W. Europe Standard Time"); // TimeZone.
				stringBuilder.AppendLine("-FD[From Date]"); // Date From.
				stringBuilder.AppendLine("-TD[To Date]"); // Date To.
				stringBuilder.AppendLine("-BU[BusinessUnit]"); // BusinessUnit Name.
				stringBuilder.AppendLine("-CO Convert Mode, new is default (leave this out if creating new)"); // Convert.
				stringBuilder.AppendLine("-CU[Culture] eg. kn-IN"); // Culture.
				stringBuilder.AppendLine("-EE Use integrated security, overrides sql login and password");
				return stringBuilder.ToString();
			}
		}

		public static string VersionInfo
		{
			get
			{
				Version version = Assembly.GetExecutingAssembly().GetName().Version;
				string versionNumber = version.ToString();

				return String.Format(CultureInfo.CurrentCulture,
						     "Teleopti CCC, version 7 Application Configurator version {0}", versionNumber);
			}
		}

		public void LogOn(ICommandLineArgument argument, DatabaseHandler databaseHandler, IBusinessUnit businessUnit)
		{
			InitializeApplication initializeApplication = new InitializeApplication(new DataSourcesFactory(new EnversConfiguration(), new List<IMessageSender>(), DataSourceConfigurationSetter.ForApplicationConfig()), null);
			initializeApplication.Start(new StateNewVersion(), databaseHandler.DataSourceSettings(), "", new ConfigurationManagerWrapper());

			AvailableDataSourcesProvider availableDataSourcesProvider =
			    new AvailableDataSourcesProvider(StateHolderReader.Instance.StateReader.ApplicationScopeData);
			var repositoryFactory = new RepositoryFactory();
			var passwordPolicy = new DummyPasswordPolicy();
			ApplicationDataSourceProvider applicationDataSourceProvider =
			    new ApplicationDataSourceProvider(availableDataSourcesProvider, repositoryFactory,
							      new FindApplicationUser(
								  new CheckNullUser(
								      new CheckSuperUser(
									  new FindUserDetail(
									      new CheckUserDetail(
										  new CheckPassword(new OneWayEncryption(), new CheckBruteForce(passwordPolicy), new CheckPasswordChange(passwordPolicy))),
									      repositoryFactory), new SystemUserSpecification(),
									  new SystemUserPasswordSpecification())), repositoryFactory));
			DataSourceContainer dataSourceContainer = applicationDataSourceProvider.DataSourceList().First();

			var logOnOff = new LogOnOff(new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory()));
			dataSourceContainer.LogOn(SuperUser.UserName, SuperUser.Password);

			logOnOff.LogOn(dataSourceContainer.DataSource, dataSourceContainer.User, businessUnit);

			var unitOfWorkFactory = dataSourceContainer.DataSource.Application;
			var roleToPrincipalCommand =
				new RoleToPrincipalCommand(
					new RoleToClaimSetTransformer(
						new FunctionsForRoleProvider(
							new DummyLicensedFunctionsProvider(),
							new ExternalFunctionsProvider(repositoryFactory)
							)
						)
					);
			using (var uow = unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				roleToPrincipalCommand.Execute(TeleoptiPrincipal.Current, uow, repositoryFactory.CreatePersonRepository(uow));
			}

			Thread.CurrentThread.CurrentCulture = argument.CultureInfo;
			Thread.CurrentThread.CurrentUICulture = argument.CultureInfo;
		}

		public void CreateNewEmptyCcc7(DefaultAggregateRoot defaultAggregateRoot)
		{
			//Create standard absences
            AbsenceCreator absenceCreator = new AbsenceCreator();
			IAbsence defaultAbsence = absenceCreator.Create(new Description("Default", "DE"), Color.DarkViolet, 1, true);

			//Create standard activities
			ActivityCreator activityCreator = new ActivityCreator();
			IActivity defaultActivity = activityCreator.Create("Default", new Description("Default"), Color.DeepSkyBlue, true, true);

			//Create standard contract
			ContractCreator contractCreator = new ContractCreator();
			IContract contract = contractCreator.Create("Fixed Staff", new Description("Fixed Staff"), EmploymentType.FixedStaffNormalWorkTime,
								    new WorkTime(new TimeSpan(8, 0, 0)),
								    new WorkTimeDirective(new TimeSpan(40, 0, 0),
											  new TimeSpan(12, 0, 0),
											  new TimeSpan(50, 0, 0)));
			//Create standard scenario
			ScenarioCreator scenarioCreator = new ScenarioCreator();
			IScenario scenario = scenarioCreator.Create("Default", new Description("Default"), true, true, false);

			//This path ends up on the root where the dll's are stored /Peter /David
			RtaStateGroupCreator rtaStateGroupCreator = new RtaStateGroupCreator(@"RtaStates.xml");

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new AbsenceRepository(uow).Add(defaultAbsence);
				new ActivityRepository(uow).Add(defaultActivity);
				new ContractRepository(uow).Add(contract);
				new ScenarioRepository(uow).Add(scenario);
				new RtaStateGroupRepository(uow).AddRange(rtaStateGroupCreator.RtaGroupCollection);
				uow.PersistAll();
			}

			//Fix settings, finns ingen nhib mappning ?!?!?
		}

		/// <summary>
		/// Gets the default aggregate root.
		/// </summary>
		/// <param name="argument">The argument.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2008-11-17
		/// </remarks>
		public DefaultAggregateRoot GetDefaultAggregateRoot(ICommandLineArgument argument)
		{
			DatabaseHandler databaseHandler = new DatabaseHandler(argument);
			DefaultDataCreator defaultDataCreator = new DefaultDataCreator(argument.BusinessUnit,
			                                                               argument.CultureInfo,
			                                                               argument.TimeZone,
			                                                               argument.NewUserName,
			                                                               argument.NewUserPassword,
			                                                               databaseHandler.SessionFactory);

			DefaultAggregateRoot defaultAggregateRoot = defaultDataCreator.Create();
			defaultDataCreator.Save(defaultAggregateRoot);

			LogOn(argument, databaseHandler, defaultAggregateRoot.BusinessUnit);
			return defaultAggregateRoot;
		}

		public static bool CheckRaptorCompatibility()
		{
			return DBConverter.DatabaseConvert.CheckRaptorCompatibility();
		}
	}

	internal class DummyLicensedFunctionsProvider : ILicensedFunctionsProvider
	{
		public IEnumerable<IApplicationFunction> LicensedFunctions()
		{
			return new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList;
		}
	}
}