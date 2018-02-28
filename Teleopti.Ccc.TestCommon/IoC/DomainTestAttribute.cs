using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.Rta.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Domain.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Rta.Service;
using Teleopti.Ccc.Domain.Rta.Tracer;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.Services;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class DomainTestAttribute : IoCTestAttribute
	{
		public static string DefaultTenantName = "default";
		public static Guid DefaultBusinessUnitId = Guid.NewGuid();

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			//TODO: move this to CommonModule
			// Really, adding domain services here is a very bad idea
			// because it will give false positives in the build
			// even though it may fail in some clients
			// all services should be added to the 
			// CommonModule !!
			system.AddModule(new OutboundScheduledResourcesProviderModule());
			system.AddService<UserDeviceService>();
			//

			system.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
			system.UseTestDouble<PersonRequestAuthorizationCheckerForTest>().For<IPersonRequestCheckAuthorization>();
			system.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();

			// Tenant (and datasource) stuff
			var tenants = new FakeTenants();
			tenants.Has(DefaultTenantName, LegacyAuthenticationKey.TheKey);
			system.UseTestDouble(tenants).For<IFindTenantNameByRtaKey, ICountTenants, ILoadAllTenants, IFindTenantByName, IAllTenantNames>();
			system.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			system.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
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

			// Database stuff
			system.UseTestDouble<FakeDistributedLockAcquirer>().For<IDistributedLockAcquirer>();
			//

			// Messaging ztuff 
			system.UseTestDouble<FakeMessageBrokerUnitOfWorkScope>().For<IMessageBrokerUnitOfWorkScope>();
			system.UseTestDouble<FakeSignalR>().For<ISignalR>();
			system.UseTestDouble<FakeMailboxRepository>().For<IMailboxRepository>();
			//

			// Rta
			system.AddService<FakeDataSources>();
			system.UseTestDouble<FakeStateQueue>().For<IStateQueueWriter, IStateQueueReader>();

			system.UseTestDouble<FakeDataSourceReader>().For<IDataSourceReader>();
			system.UseTestDouble<FakeExternalLogonReadModelPersister>().For<IExternalLogonReader, IExternalLogonReadModelPersister>();
			system.UseTestDouble<FakeMappingReadModelPersister>().For<IMappingReader, IMappingReadModelPersister>();
			system.UseTestDouble<FakeCurrentScheduleReadModelPersister>().For<IScheduleReader, ICurrentScheduleReadModelPersister>();
			system.UseTestDouble<FakeAgentStatePersister>().For<IAgentStatePersister>();

			system.UseTestDouble<FakeAgentStateReadModelPersister>().For<IAgentStateReadModelPersister, IAgentStateReadModelReader>();
			system.UseTestDouble<FakeHistoricalAdherenceReadModelPersister>().For<IHistoricalAdherenceReadModelReader, IHistoricalAdherenceReadModelPersister>();
			system.UseTestDouble<FakeHistoricalChangeReadModelPersister>().For<IHistoricalChangeReadModelPersister, IHistoricalChangeReadModelReader>();
			system.UseTestDouble<FakeApprovedPeriodsStorage>().For<IApprovedPeriodsReader, IApprovedPeriodsPersister>();;
			
			system.UseTestDouble<FakeAllLicenseActivatorProvider>().For<ILicenseActivatorProvider>();
			system.UseTestDouble<FakeTeamCardReader>().For<ITeamCardReader>();
			system.UseTestDouble<FakeOrganizationReader>().For<IOrganizationReader>();
			system.UseTestDouble<FakeRtaTracerPersister>().For<IRtaTracerReader, IRtaTracerWriter>();
			system.UseTestDouble<FakeRtaTracerConfigPersister>().For<IRtaTracerConfigPersister>();
			//

			system.UseTestDouble<NoScheduleRangeConflictCollector>().For<IScheduleRangeConflictCollector>();

			// readmodels
			system.UseTestDouble<FakeScheduleProjectionReadOnlyPersister>().For<IScheduleProjectionReadOnlyPersister>();
			system.UseTestDouble<FakeProjectionVersionPersister>().For<IProjectionVersionPersister>();
			system.UseTestDouble<FakeKeyValueStorePersister>().For<IKeyValueStorePersister>();
			system.UseTestDouble<FakePersonAssociationPublisherCheckSumPersister>().For<IPersonAssociationPublisherCheckSumPersister>();


			// AppInsights
			system.UseTestDouble<FakeApplicationInsights>().For<IApplicationInsights>();
			system.UseTestDouble<FakeApplicationInsightsConfigReader>().For<IApplicationInsightsConfigurationReader>();

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
				system.UseTestDouble<FakeSkillCombinationResourceReader>().For<ISkillCombinationResourceReader>();
				system.UseTestDouble<FakePersonRepository>().For<IPersonRepository, IProxyForId<IPerson>, IPersonLoadAllWithPeriodAndExternalLogOn>();
				system.UseTestDouble<FakeMultisiteDayRepository>().For<IMultisiteDayRepository>();
				system.UseTestDouble<FakeBusinessUnitRepository>().For<IBusinessUnitRepository>();
				system.UseTestDouble<FakeApplicationRoleRepository>().For<IApplicationRoleRepository>();
				system.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
				system.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
				system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
				system.UseTestDouble<FakeGroupPageRepository>().For<IGroupPageRepository>();
				system.UseTestDouble<FakeExternalLogOnRepository>().For<IExternalLogOnRepository>();
				system.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
				system.UseTestDouble<FakeActivityRepository>().For<IActivityRepository, IProxyForId<IActivity>>();
				system.UseTestDouble<FakePlanningPeriodRepository>().For<IPlanningPeriodRepository>();
				system.UseTestDouble<FakeExistingForecastRepository>().For<IExistingForecastRepository>();
				system.UseTestDouble<FakeDayOffTemplateRepository>().For<IDayOffTemplateRepository>();
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
				system.UseTestDouble<FakePlanningGroupSettingsRepository>().For<IPlanningGroupSettingsRepository>();
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
				system.UseTestDouble<FakeSkillGroupRepository>().For<ISkillGroupRepository>();
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
				system.UseTestDouble<FakeGlobalSettingDataRepository>().For<IGlobalSettingDataRepository>();
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
				system.UseTestDouble<FakeAnalyticsSiteRepository>().For<IAnalyticsSiteRepository>();
				system.UseTestDouble<FakeAnalyticsTimeZoneRepository>().For<IAnalyticsTimeZoneRepository>();
				system.UseTestDouble<FakeAnalyticsWorkloadRepository>().For<IAnalyticsWorkloadRepository>();
				system.UseTestDouble<FakeUserDeviceRepository>().For<IUserDeviceRepository>();
				system.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
				system.UseTestDouble<FakeScheduleAuditTrailReport>().For<IScheduleAuditTrailReport>();
				system.UseTestDouble<FakeBudgetGroupRepository>().For<IBudgetGroupRepository>();
				system.UseTestDouble<FakeBudgetDayRepository>().For<IBudgetDayRepository>();
				system.UseTestDouble<FakePersonFinderReadOnlyRepository>().For<IPersonFinderReadOnlyRepository>();

				system.UseTestDouble<FakeSeatBookingRepository>().For<ISeatBookingRepository>();
				system.UseTestDouble<FakeSeatMapRepository>().For<ISeatMapLocationRepository>();
				system.UseTestDouble<FakeSeatPlanRepository>().For<ISeatPlanRepository>();
				system.UseTestDouble<SeatPlanner>().For<ISeatPlanner>();
				system.UseTestDouble<SeatMapPersister>().For<ISeatMapPersister>();
				system.UseTestDouble<SeatPlanPersister>().For<ISeatPlanPersister>();
				system.UseTestDouble<SeatBookingRequestAssembler>().For<ISeatBookingRequestAssembler>();
				system.UseTestDouble<SeatFrequencyCalculator>().For<ISeatFrequencyCalculator>();
				system.UseTestDouble<FakeOptionalColumnRepository>().For<IOptionalColumnRepository>();
			}
			system.UseTestDouble<ScheduleStorageRepositoryWrapper>().For<IScheduleStorageRepositoryWrapper>();
			system.AddService<FakeSchedulingSourceScope>();

			if (QueryAllAttributes<LoggedOnAppDomainAttribute>().Any())
				system.UseTestDouble<FakeAppDomainPrincipalContext>().For<IThreadPrincipalContext>();
			else
				system.UseTestDouble<FakeThreadPrincipalContext>().For<IThreadPrincipalContext>();

			if (fullPermissions())
				system.UseTestDouble<FullPermission>().For<IAuthorization>();
			if (fakePermissions())
				system.UseTestDouble<FakePermissions>().For<IAuthorization>();
		}

		public IAuthorizationScope AuthorizationScope;
		public IAuthorization Authorization;
		public IThreadPrincipalContext PrincipalContext;
		public FakeDataSourceForTenant DataSourceForTenant;
		public IDataSourceScope DataSourceScope;
		public IPersonRepository Persons;
		public Lazy<FakeDatabase> Database;

		private IDisposable _authorizationScope;
		private IDisposable _tenantScope;
		private Person _loggedOnPerson;

		protected override void BeforeTest()
		{
			base.BeforeTest();

			fakeSignin();

			createDefaultData();

			// because DomainTest project has OneTimeSetUp that sets FullPermissions globally... 
			// ... we need to scope real/fake/full for this test
			if (realPermissions())
				_authorizationScope = AuthorizationScope.OnThisThreadUse((PrincipalAuthorization)Authorization);
			else if (fakePermissions())
				_authorizationScope = AuthorizationScope.OnThisThreadUse((FakePermissions)Authorization);
			else
				_authorizationScope = AuthorizationScope.OnThisThreadUse((FullPermission)Authorization);
		}

		private void fakeSignin()
		{
			var signedIn = QueryAllAttributes<LoggedOffAttribute>().IsEmpty();
			if (signedIn)
			{
				_loggedOnPerson = new Person()
					.WithName(new Name("Fake", "Login"))
					.WithId();
				_loggedOnPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
				_loggedOnPerson.PermissionInformation.SetCulture(CultureInfoFactory.CreateEnglishCulture());
				_loggedOnPerson.PermissionInformation.SetUICulture(CultureInfoFactory.CreateEnglishCulture());

				var principal = new TeleoptiPrincipal(
					new TeleoptiIdentity(
						"Fake Login",
						DataSourceForTenant.Tenant(DefaultTenantName),
						new BusinessUnit("loggedOnBu").WithId(),
						null,
						null
					),
					_loggedOnPerson);

				PrincipalContext.SetCurrentPrincipal(principal);
			}
			else
			{
				// assuming that if you specify default data you are using FakeDatabase...
				// ... and thereby you want a current tenant for it to work...
				if (QueryAllAttributes<DefaultDataAttribute>().Any())
					_tenantScope = DataSourceScope.OnThisThreadUse(DefaultTenantName);
			}
		}

		private void createDefaultData()
		{
			if (_loggedOnPerson != null)
				(Persons as FakePersonRepository)?.Has(_loggedOnPerson);

			if (QueryAllAttributes<DefaultDataAttribute>().Any())
				Database.Value.CreateDefaultData();
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

			_tenantScope?.Dispose();
			_authorizationScope?.Dispose();
		}
	}

}