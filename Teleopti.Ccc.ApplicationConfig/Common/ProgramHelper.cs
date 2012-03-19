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
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Composites;

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "If we want to really work with this we should use Autofac to supply the stuff we need!")]
        public void LogOn(ICommandLineArgument argument, DatabaseHandler databaseHandler, IBusinessUnit businessUnit, IPerson convertPerson)
        {
			InitializeApplication initializeApplication = new InitializeApplication(new DataSourcesFactory(new EnversConfiguration(), new List<IDenormalizer>()),
				MessageBrokerImplementation.GetInstance(MessageFilterManager.Instance.FilterDictionary));
            initializeApplication.Start(new StateNewVersion(), databaseHandler.DataSourceSettings(), "");

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
                                                                      new CheckPassword(new OneWayEncryption(),new CheckBruteForce(passwordPolicy), new CheckPasswordChange(passwordPolicy))),
                                                                  repositoryFactory), new SystemUserSpecification(),
                                                              new SystemUserPasswordSpecification())), repositoryFactory));
            DataSourceContainer dataSourceContainer = applicationDataSourceProvider.DataSourceList().First();

			var logOnOff = new LogOnOff(new PrincipalManager());
            dataSourceContainer.LogOn(
                convertPerson.PermissionInformation.ApplicationAuthenticationInfo.ApplicationLogOnName,
                convertPerson.PermissionInformation.ApplicationAuthenticationInfo.Password);

            logOnOff.LogOn(dataSourceContainer.DataSource, dataSourceContainer.User, businessUnit, AuthenticationTypeOption.Application);

            var unitOfWorkFactory = dataSourceContainer.DataSource.Application;
            RoleToPrincipalCommand roleToPrincipalCommand =
                new RoleToPrincipalCommand(
                    new RoleToClaimSetTransformer(new FunctionsForRoleProvider(new DummyLicensedFunctionsProvider(),
                                                                               new ExternalFunctionsProvider(repositoryFactory))));
			using(var uow = unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
			    roleToPrincipalCommand.Execute(TeleoptiPrincipal.Current, uow, repositoryFactory.CreatePersonRepository(uow));
			}

            Thread.CurrentThread.CurrentCulture = argument.CultureInfo;
            Thread.CurrentThread.CurrentUICulture = argument.CultureInfo;
        }

        public void CreateNewEmptyCcc7(DefaultAggregateRoot defaultAggregateRoot)
        {
            //Create standard absences
            AbsenceCreator absenceCreator = new AbsenceCreator(defaultAggregateRoot.GroupingAbsence);
            IAbsence defaultAbsence = absenceCreator.Create(new Description("Default", "DE"), Color.DarkViolet, 1, true);
            
            //Create standard activities
            ActivityCreator activityCreator = new ActivityCreator(defaultAggregateRoot.GroupingActivity);
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
            IScenario scenario = scenarioCreator.Create("Default", new Description("Default"), true,true, false);

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
                                                                           databaseHandler.SessionFactory);
 
            DefaultAggregateRoot defaultAggregateRoot = defaultDataCreator.Create();
            defaultDataCreator.Save(defaultAggregateRoot);
            
            IPerson convertPerson = defaultDataCreator.ConvertPerson;

            LogOn(argument, databaseHandler, defaultAggregateRoot.BusinessUnit, convertPerson);
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