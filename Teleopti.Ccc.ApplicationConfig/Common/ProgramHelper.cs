using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.ApplicationConfig.Common
{
	public class ProgramHelper
	{
		public static string UsageInfo
		{
			get
			{
				var stringBuilder = new StringBuilder();
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
							  "Teleopti WFM, Application Configurator version {0}", versionNumber);
			}
		}

		public void LogOn(ICommandLineArgument argument, DatabaseHandler databaseHandler, IBusinessUnit businessUnit)
		{
			var dataSourcesFactory = new DataSourcesFactory(
				new EnversConfiguration(),
				new NoMessageSenders(),
				DataSourceConfigurationSetter.ForApplicationConfig(),
				new CurrentHttpContext(),
				() => StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging
				);
			var dataSource = dataSourcesFactory.Create(databaseHandler.DataSourceSettings(), "");
			var dsForTenant = new DataSourceForTenant(dataSourcesFactory);

			var state = new StateNewVersion();
			var applicationData = new ApplicationData(ConfigurationManager.AppSettings.ToDictionary(), null, null, dsForTenant);
			dsForTenant.MakeSureDataSourceExists_UseOnlyFromTests(dataSource); //no threading issues so it should be fine
			state.SetApplicationData(applicationData);
			StateHolder.Initialize(state);

			var repositoryFactory = new RepositoryFactory();

			var unitOfWorkFactory = dataSource.Application;
			var logOnOff = new LogOnOff(new WindowsAppDomainPrincipalContext(new TeleoptiPrincipalFactory()));
			var user = new LoadUserUnauthorized().LoadFullPersonInSeperateTransaction(unitOfWorkFactory, SuperUser.Id_AvoidUsing_This);
			
			logOnOff.LogOn(dataSource, user, businessUnit);

			var roleToPrincipalCommand =
				new RoleToPrincipalCommand(
					new RoleToClaimSetTransformer(new FunctionsForRoleProvider(new DummyLicensedFunctionsProvider(),
						new ExternalFunctionsProvider(repositoryFactory))));
			using (var uow = unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				roleToPrincipalCommand.Execute(TeleoptiPrincipal.CurrentPrincipal, unitOfWorkFactory, repositoryFactory.CreatePersonRepository(uow));
			}

			Thread.CurrentThread.CurrentCulture = argument.CultureInfo;
			Thread.CurrentThread.CurrentUICulture = argument.CultureInfo;
		}

		public void CreateNewEmptyCcc7(Action saveBusinessUnitAction)
		{
			//Create standard absences
			var absenceCreator = new AbsenceCreator();
			IAbsence defaultAbsence = absenceCreator.Create(new Description("Default", "DE"), Color.DarkViolet, 1, true);

			//Create standard activities
			var activityCreator = new ActivityCreator();
			IActivity defaultActivity = activityCreator.Create("Default", new Description("Default"), Color.DeepSkyBlue, true, true);

			//Create standard contract
			var contractCreator = new ContractCreator();
			IContract contract = contractCreator.Create("Fixed Staff", new Description("Fixed Staff"),
				EmploymentType.FixedStaffNormalWorkTime, new WorkTime(new TimeSpan(8, 0, 0)),
				new WorkTimeDirective(new TimeSpan(0, 0, 0), new TimeSpan(40, 0, 0), new TimeSpan(12, 0, 0), new TimeSpan(50, 0, 0)));
			//Create standard scenario
			var scenarioCreator = new ScenarioCreator();
			IScenario scenario = scenarioCreator.Create("Default", new Description("Default"), true, true, false);

			//This path ends up on the root where the dll's are stored /Peter /David
			var rtaStateGroupCreator = new RtaStateGroupCreator(@"RtaStates.xml");

			saveBusinessUnitAction.Invoke();

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

		public DefaultAggregateRoot GetDefaultAggregateRoot(ICommandLineArgument argument)
		{
			var databaseHandler = new DatabaseHandler(argument);
			var defaultDataCreator = new DefaultDataCreator(argument.BusinessUnit, argument.CultureInfo, argument.TimeZone,
				argument.NewUserName, argument.NewUserPassword, databaseHandler.SessionFactory, TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(argument.DestinationConnectionString));

			DefaultAggregateRoot defaultAggregateRoot = defaultDataCreator.Create();
			defaultDataCreator.Save(defaultAggregateRoot);

			LogOn(argument, databaseHandler, defaultAggregateRoot.BusinessUnit);
			return defaultAggregateRoot;
		}
	}

	internal class DummyLicensedFunctionsProvider : ILicensedFunctionsProvider
	{
		public IEnumerable<IApplicationFunction> LicensedFunctions(string dataSourceName)
		{
			return new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList;
		}
	}
}