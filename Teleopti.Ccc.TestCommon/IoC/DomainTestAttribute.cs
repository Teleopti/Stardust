using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.Services;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[Toggle(Domain.FeatureFlags.Toggles.RTA_SpreadTransactionLocksStrategy_41823)]
	[Toggle(Domain.FeatureFlags.Toggles.Wfm_Requests_Check_Expired_Requests_40274)]
	public class DomainTestAttribute : IoCTestAttribute
	{
		public static string DefaultTenantName = "default";

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			//TODO: move this to common
			system.AddModule(new OutboundScheduledResourcesProviderModule());
			//

			system.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
			system.UseTestDouble<PersonRequestAuthorizationCheckerForTest>().For<IPersonRequestCheckAuthorization>();

			// Tenant (and datasource) stuff
			system.AddModule(new TenantServerModule(configuration));
			system.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			system.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			var tenants = new FakeTenants();
			tenants.Has(DefaultTenantName, LegacyAuthenticationKey.TheKey);
			system.UseTestDouble(tenants)
				.For<IFindTenantNameByRtaKey, ICountTenants, ILoadAllTenants, IFindTenantByName, IAllTenantNames>();
			system.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();
			system.UseTestDouble<FakeDataSourcesFactory>().For<IDataSourcesFactory>();
			//

			// Event stuff
			system.UseTestDouble<FakeMessageSender>().For<IMessageSender>();
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
			QueryAllAttributes<UseEventPublisherAttribute>()
				.ForEach(a => system.UseTestDoubleForType(a.EventPublisher).For<IEventPublisher>());
			system.UseTestDouble<FakeRecurringEventPublisher>().For<IRecurringEventPublisher>();
			system.UseTestDouble<EmptyStardustJobFeedback>().For<IStardustJobFeedback>();
			//

			// Database aspects
			system.UseTestDouble<FakeReadModelUnitOfWorkAspect>().For<IReadModelUnitOfWorkAspect>();
			system.UseTestDouble<FakeAllBusinessUnitsUnitOfWorkAspect>().For<IAllBusinessUnitsUnitOfWorkAspect>();
			system.UseTestDouble<FakeDistributedLockAcquirer>().For<IDistributedLockAcquirer>();
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble<FakeCurrentAnalyticsUnitOfWorkFactory>().For<ICurrentAnalyticsUnitOfWorkFactory>();
			system.UseTestDouble<FakeMessageBrokerUnitOfWorkAspect>().For<IMessageBrokerUnitOfWorkAspect>();
			//

			// Messaging ztuff 
			system.UseTestDouble<FakeSignalR>().For<ISignalR>();
			//system.UseTestDouble<ActionImmediate>().For<IActionScheduler>();
			system.UseTestDouble<FakeMailboxRepository>().For<IMailboxRepository>();
			//

			// Rta
			system.AddService<FakeDataSources>();
			system.AddService<FakeRtaDatabase>();
			system.UseTestDouble<FakeStateQueue>().For<IStateQueueWriter, IStateQueueReader>();

			system.UseTestDouble<FakeDataSourceReader>().For<IDataSourceReader>();
			system.UseTestDouble<FakeExternalLogonReadModelPersister>().For<IExternalLogonReader, IExternalLogonReadModelPersister>();
			system.UseTestDouble<FakeMappingReadModelPersister>().For<IMappingReader, IMappingReadModelPersister>();
			system.UseTestDouble<FakeCurrentScheduleReadModelPersister>().For<IScheduleReader, ICurrentScheduleReadModelPersister>();
			system.UseTestDouble<FakeAgentStatePersister>().For<IAgentStatePersister>();

			system.UseTestDouble<FakeAgentStateReadModelPersister>().For<IAgentStateReadModelPersister, IAgentStateReadModelReader>();
			system.UseTestDouble<FakeHistoricalAdherenceReadModelPersister>().For<IHistoricalAdherenceReadModelReader, IHistoricalAdherenceReadModelPersister>();
			system.UseTestDouble<FakeHistoricalChangeReadModelPersister>().For<IHistoricalChangeReadModelPersister, IHistoricalChangeReadModelReader>();
			system.UseTestDouble<FakeAdherencePercentageReadModelPersister>().For<IAdherencePercentageReadModelPersister, IAdherencePercentageReadModelReader>();
			system.UseTestDouble<FakeNumberOfAgentsInSiteReader>().For<INumberOfAgentsInSiteReader>();
			system.UseTestDouble<FakeNumberOfAgentsInTeamReader>().For<INumberOfAgentsInTeamReader>();
			system.UseTestDouble<FakeAllLicenseActivatorProvider>().For<ILicenseActivatorProvider>();
			system.UseTestDouble<FakeTeamCardReader>().For<ITeamCardReader>();
			system.UseTestDouble<FakeOrganizationReader>().For<IOrganizationReader>();
			//

			system.UseTestDouble<FakeScheduleRangePersister>().For<IScheduleRangePersister>();

			// readmodels
			system.UseTestDouble<FakeScheduleProjectionReadOnlyPersister>().For<IScheduleProjectionReadOnlyPersister>();
			system.UseTestDouble<FakeProjectionVersionPersister>().For<IProjectionVersionPersister>();
			system.UseTestDouble<FakeKeyValueStorePersister>().For<IKeyValueStorePersister>();
			system.UseTestDouble<FakePersonAssociationPublisherCheckSumPersister>().For<IPersonAssociationPublisherCheckSumPersister>();

			// licensing
			system.UseTestDouble<FakeLicenseRepository>().For<ILicenseRepository, ILicenseRepositoryForLicenseVerifier>();

			// Repositories
			system.AddService<FakeDatabase>();
			system.AddService<FakeStorage>();
			if (QueryAllAttributes<ThrowIfRepositoriesAreUsedAttribute>().Any())
			{
				SetupThrowingTestDoublesForRepositories.Execute(system);
			}
			else
			{
				system.UseTestDouble<FakeMultisiteDayRepository>().For<IMultisiteDayRepository>();
				system.UseTestDouble<FakeBusinessUnitRepository>().For<IBusinessUnitRepository>();
				system.UseTestDouble<FakeApplicationRoleRepository>().For<IApplicationRoleRepository>();
				system.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
				system.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
				system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
				system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
				system.UseTestDouble<FakeGroupPageRepository>().For<IGroupPageRepository>();
				system.UseTestDouble<FakeExternalLogOnRepository>().For<IExternalLogOnRepository>();
				system.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
				system.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
				system.UseTestDouble<FakePlanningPeriodRepository>().For<IPlanningPeriodRepository>();
				system.UseTestDouble<FakeExistingForecastRepository>().For<IExistingForecastRepository>();
				system.UseTestDouble<FakeDayOffTemplateRepository>().For<IDayOffTemplateRepository>();
				system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
				system.UseTestDouble<FakeRepositoryFactory>().For<IRepositoryFactory>();
				system.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
				system.UseTestDouble<FakeAbsenceRepository>().For<IAbsenceRepository>();
				system.UseTestDouble<FakeShiftCategoryRepository>().For<IShiftCategoryRepository>();
				system.UseTestDouble<FakeContractRepository>().For<IContractRepository>();
				system.UseTestDouble<FakeContractScheduleRepository>().For<IContractScheduleRepository>();
				system.UseTestDouble<FakeScheduleTagRepository>().For<IScheduleTagRepository>();
				system.UseTestDouble<FakeWorkflowControlSetRepository>().For<IWorkflowControlSetRepository>();
				system.UseTestDouble<FakeMeetingRepository>().For<IMeetingRepository>();
				system.UseTestDouble<FakeAgentDayScheduleTagRepository>().For<IAgentDayScheduleTagRepository>();
				system.UseTestDouble<FakePreferenceDayRepository>().For<IPreferenceDayRepository>();
				system.UseTestDouble<FakeStudentAvailabilityDayRepository>().For<IStudentAvailabilityDayRepository>();
				system.UseTestDouble<FakeOvertimeAvailabilityRepository>().For<IOvertimeAvailabilityRepository>();
				system.UseTestDouble<FakePersonAvailabilityRepository>().For<IPersonAvailabilityRepository>();
				system.UseTestDouble<FakePersonRotationRepository>().For<IPersonRotationRepository>();
				system.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
				system.UseTestDouble<FakeDayOffRulesRepository>().For<IDayOffRulesRepository>();
				system.UseTestDouble<FakePlanningGroupRepository>().For<IPlanningGroupRepository>();
				system.UseTestDouble<FakeStatisticRepository>().For<IStatisticRepository>();
				system.UseTestDouble<FakeRtaStateGroupRepository>().For<IRtaStateGroupRepository>();
				system.UseTestDouble<FakeRtaMapRepository>().For<IRtaMapRepository>();
				system.UseTestDouble<FakeRtaRuleRepository>().For<IRtaRuleRepository>();
				system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
				system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
				system.UseTestDouble<FakePartTimePercentageRepository>().For<IPartTimePercentageRepository>();
				system.UseTestDouble<FakeMultiplicatorDefinitionSetRepository>().For<IMultiplicatorDefinitionSetRepository>();
				system.UseTestDouble<FakeIntradayMonitorDataLoader>().For<IIntradayMonitorDataLoader>();
				system.UseTestDouble<FakeApplicationFunctionRepository>().For<IApplicationFunctionRepository>();
				system.UseTestDouble<FakeAvailableDataRepository>().For<IAvailableDataRepository>();
				system.UseTestDouble<FakeIntervalLengthFetcher>().For<IIntervalLengthFetcher>();
				system.UseTestDouble<FakeSkillAreaRepository>().For<ISkillAreaRepository>();
				system.UseTestDouble<FakeLoadAllSkillInIntradays>().For<ILoadAllSkillInIntradays>();
				system.UseTestDouble<FakeGroupingReadOnlyRepository>().For<IGroupingReadOnlyRepository>();
				system.UseTestDouble<FakeCommonAgentNameProvider>().For<ICommonAgentNameProvider>();
				system.UseTestDouble<FakePersonRequestRepository>().For<IPersonRequestRepository>();
				system.UseTestDouble<FakeQueuedAbsenceRequestRepository>().For<IQueuedAbsenceRequestRepository>();
				system.UseTestDouble<FakeRequestStrategySettingsReader>().For<IRequestStrategySettingsReader>();
				system.UseTestDouble<FakeLatestStatisticsIntervalIdLoader>().For<ILatestStatisticsIntervalIdLoader>();
				system.UseTestDouble<FakeRuleSetBagRepository>().For<IRuleSetBagRepository>();
				system.UseTestDouble<FakeIntradayQueueStatisticsLoader>().For<IIntradayQueueStatisticsLoader>();
				system.UseTestDouble<FakeNoteRepository>().For<INoteRepository>();
				system.UseTestDouble<FakePublicNoteRepository>().For<IPublicNoteRepository>();
				system.UseTestDouble<FakeWorkloadRepository>().For<IWorkloadRepository>();
				system.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>();
				system.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
				system.UseTestDouble<FakeJobResultRepository>().For<IJobResultRepository>();
				system.UseTestDouble<FakeJobStartTimeRepository>().For<IJobStartTimeRepository>();
				system.UseTestDouble<FakeAnalyticsAbsenceRepository>().For<IAnalyticsAbsenceRepository>();
				system.UseTestDouble<FakeAnalyticsActivityRepository>().For<IAnalyticsActivityRepository>();
				system.UseTestDouble<FakeAnalyticsBridgeGroupPagePersonRepository>().For<IAnalyticsBridgeGroupPagePersonRepository>();
				system.UseTestDouble<FakeAnalyticsBridgeTimeZoneRepository>().For<IAnalyticsBridgeTimeZoneRepository>();
				system.UseTestDouble<FakeAnalyticsBusinessUnitRepository>().For<IAnalyticsBusinessUnitRepository>();
				system.UseTestDouble<FakeAnalyticsDateRepository>().For<IAnalyticsDateRepository>();
				system.UseTestDouble<FakeAnalyticsDayOffRepository>().For<IAnalyticsDayOffRepository>();
				system.UseTestDouble<FakeAnalyticsForecastWorkloadRepository>().For<IAnalyticsForecastWorkloadRepository>();
				system.UseTestDouble<FakeAnalyticsGroupPageRepository>().For<IAnalyticsGroupPageRepository>();
				system.UseTestDouble<FakeAnalyticsHourlyAvailabilityRepository>().For<IAnalyticsHourlyAvailabilityRepository>();
				system.UseTestDouble<FakeAnalyticsIntervalRepository>().For<IAnalyticsIntervalRepository>();
				system.UseTestDouble<FakeAnalyticsOvertimeRepository>().For<IAnalyticsOvertimeRepository>();
				system.UseTestDouble<FakeAnalyticsPermissionExecutionRepository>().For<IAnalyticsPermissionExecutionRepository>();
				system.UseTestDouble<FakeAnalyticsPermissionRepository>().For<IAnalyticsPermissionRepository>();
				system.UseTestDouble<FakeAnalyticsPersonPeriodRepository>().For<IAnalyticsPersonPeriodRepository>();
				system.UseTestDouble<FakeAnalyticsPreferenceRepository>().For<IAnalyticsPreferenceRepository>();
				system.UseTestDouble<FakeAnalyticsRequestRepository>().For<IAnalyticsRequestRepository>();
				system.UseTestDouble<FakeAnalyticsScenarioRepository>().For<IAnalyticsScenarioRepository>();
				system.UseTestDouble<FakeAnalyticsScheduleRepository>().For<IAnalyticsScheduleRepository>();
				system.UseTestDouble<FakeAnalyticsShiftCategoryRepository>().For<IAnalyticsShiftCategoryRepository>();
				system.UseTestDouble<FakeAnalyticsSkillRepository>().For<IAnalyticsSkillRepository>();
				system.UseTestDouble<FakeAnalyticsTeamRepository>().For<IAnalyticsTeamRepository>();
				system.UseTestDouble<FakeAnalyticsTimeZoneRepository>().For<IAnalyticsTimeZoneRepository>();
				system.UseTestDouble<FakeAnalyticsWorkloadRepository>().For<IAnalyticsWorkloadRepository>();
			}
			system.UseTestDouble<ScheduleStorageRepositoryWrapper>().For<IScheduleStorageRepositoryWrapper>();
			system.AddService<FakeSchedulingSourceScope>();

			fakePrincipal(system);

			if (fullPermissions())
				system.UseTestDouble<FullPermission>().For<IAuthorization>();
			if (fakePermissions())
				system.UseTestDouble<FakePermissions>().For<IAuthorization>();
		}

		private void fakePrincipal(ISystem system)
		{
			var context = QueryAllAttributes<LoggedOnAppDomainAttribute>().Any() ? 
				(IThreadPrincipalContext) new FakeAppDomainPrincipalContext() : 
				new FakeThreadPrincipalContext();

			var signedIn = QueryAllAttributes<LoggedOffAttribute>().IsEmpty();
			if (signedIn)
			{
				var person = new Person().WithName(new Name("Fake", "Login"));
				person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
				person.PermissionInformation.SetCulture(CultureInfoFactory.CreateEnglishCulture());
				person.PermissionInformation.SetUICulture(CultureInfoFactory.CreateEnglishCulture());
				var loggedOnBu = new BusinessUnit("loggedOnBu").WithId();
				var principal = new TeleoptiPrincipal(new TeleoptiIdentity("Fake Login", new FakeDataSource(DefaultTenantName), loggedOnBu, null, null), person);
				context.SetCurrentPrincipal(principal);
			}

			system.UseTestDouble(context).For<IThreadPrincipalContext>();
		}

		public IAuthorizationScope AuthorizationScope;
		public IAuthorization Authorization;
		public ICurrentTeleoptiPrincipal CurrentTeleoptiPrincipal;
		private IDisposable _authorizationScope;

		protected override void BeforeTest()
		{
			base.BeforeTest();

			// because DomainTest project has OneTimeSetUp that sets FullPermissions globally... 
			// ... we need to scope real/fake/full for this test
			if (realPermissions())
				_authorizationScope = AuthorizationScope.OnThisThreadUse((PrincipalAuthorization) Authorization);
			else if (fakePermissions())
				_authorizationScope = AuthorizationScope.OnThisThreadUse((FakePermissions) Authorization);
			else
				_authorizationScope = AuthorizationScope.OnThisThreadUse((FullPermission) Authorization);
		}

		private bool fullPermissions()
		{
			return !realPermissions() && !fakePermissions();
		}

		private bool realPermissions()
		{
			return QueryAllAttributes<RealPermissionsAttribute>().Any();
		}

		private bool fakePermissions()
		{
			return QueryAllAttributes<FakePermissionsAttribute>().Any();
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			_authorizationScope?.Dispose();
		}
	}
}