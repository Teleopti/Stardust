using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Search;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.ToggleAdmin;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.Historical;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;
using Teleopti.Wfm.Adherence.Monitor.Infrastructure;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.States.Infrastructure;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[Toggle(Domain.FeatureFlags.Toggles.RTA_AdjustAdherenceToNeutral_80594)]
	[Toggle(Domain.FeatureFlags.Toggles.RTA_ReviewHistoricalAdherence_74770)]
	public class DomainTestAttribute : IoCTestAttribute
	{
		public static string DefaultTenantName = "default";
		public static Guid DefaultBusinessUnitId = Guid.NewGuid();

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddService<FakeDataSources>();

			extend.AddService<FakeDatabase>();
			if (QueryAllAttributes<DontSendEventsAtPersistAttribute>().Any())
			{
				extend.AddService<FakeStorageSimple>();
			}
			else
			{
				extend.AddService<FakeStorage>();
			}

			extend.AddService<FakeSchedulingSourceScope>();
			extend.AddService<FakeRtaHistory>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			// stuff?
			isolate.UseTestDouble<MutableNowWithEvents>().For<MutableNow, INow, IMutateNow>();
			isolate.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();

			if (!QueryAllAttributes<UseRealPersonRequestPermissionCheckAttribute>().Any())
			{
				isolate.UseTestDouble<PersonRequestAuthorizationCheckerForTest>()
					.For<IPersonRequestCheckAuthorization>();
			}

			isolate.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();

			// Tenant (and datasource) stuff
			var tenants = new FakeTenants();
			tenants.Has(DefaultTenantName, LegacyAuthenticationKey.TheKey);
			isolate.UseTestDouble(tenants).For<IFindTenantNameByRtaKey, ICountTenants, ILoadAllTenants, IFindTenantByName, IAllTenantNames>();
			isolate.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			isolate.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();
			isolate.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();
			isolate.UseTestDouble<FakeDataSourcesFactory>().For<IDataSourcesFactory>();
			//

			// Event stuff
			isolate.UseTestDouble<FakeMessageSender>().For<IMessageSender>();
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
			isolate.UseTestDouble<ThrowExceptionsFromSyncEventPublisher>().For<ISyncEventPublisherExceptionHandler>();
			isolate.UseTestDouble<ThrowExceptionsFromRtaEventPublisher>().For<IRtaEventPublisherExceptionHandler>();
			isolate.UseTestDouble<RunSynchronouslyAndThrow>().For<IRtaEventStoreAsyncSynchronizerStrategy>();
			isolate.UseTestDouble<FakeRtaEventStore>().For<IRtaEventStore, IRtaEventStoreReader, IRtaEventStoreUpgradeWriter>();
			QueryAllAttributes<UseEventPublisherAttribute>()
				.ForEach(a => isolate.UseTestDoubleForType(a.EventPublisher).For<IEventPublisher>());
			isolate.UseTestDouble<FakeRecurringEventPublisher>().For<IRecurringEventPublisher>();
			isolate.UseTestDouble<EmptyStardustJobFeedback>().For<IStardustJobFeedback>();
			isolate.UseTestDouble<HandlerTypeMapperForTest>().For<HandlerTypeMapper>();
			//

			// Database stuff
			isolate.UseTestDouble<FakeDistributedLockAcquirer>().For<IDistributedLockAcquirer>();
			//

			// Rta
			isolate.UseTestDouble<FakeStateQueue>().For<IStateQueueWriter, IStateQueueReader>();

			isolate.UseTestDouble<FakeDataSourceReader>().For<IDataSourceReader>();
			isolate.UseTestDouble<FakeExternalLogonReadModelPersister>().For<IExternalLogonReader, IExternalLogonReadModelPersister>();
			isolate.UseTestDouble<FakeMappingReadModelPersister>().For<IMappingReader, IMappingReadModelPersister>();
			isolate.UseTestDouble<FakeCurrentScheduleReadModelPersister>().For<IScheduleReader, ICurrentScheduleReadModelPersister>();
			isolate.UseTestDouble<FakeAgentStatePersister>().For<IAgentStatePersister>();

			isolate.UseTestDouble<FakeAgentStateReadModelPersister>().For<IAgentStateReadModelPersister, IAgentStateReadModelReader>();

			isolate.UseTestDouble<FakeHistoricalOverviewReadModelPersister>().For<IHistoricalOverviewReadModelPersister, IHistoricalOverviewReadModelReader>();

			isolate.UseTestDouble<FakeAllLicenseActivatorProvider>().For<ILicenseActivatorProvider>();
			isolate.UseTestDouble<FakeTeamCardReader>().For<ITeamCardReader>();
			isolate.UseTestDouble<FakeOrganizationReader>().For<IOrganizationReader>();
			isolate.UseTestDouble<FakeRtaTracerPersister>().For<IRtaTracerReader, IRtaTracerWriter>();
			isolate.UseTestDouble<FakeRtaTracerConfigPersister>().For<IRtaTracerConfigPersister>();
			//

			isolate.UseTestDouble<NoScheduleRangeConflictCollector>().For<IScheduleRangeConflictCollector>();

			// readmodels
			isolate.UseTestDouble<FakeScheduleProjectionReadOnlyPersister>().For<IScheduleProjectionReadOnlyPersister>();
			isolate.UseTestDouble<FakeProjectionVersionPersister>().For<IProjectionVersionPersister>();
			isolate.UseTestDouble<FakeKeyValueStorePersister>().For<IKeyValueStorePersister>();
			isolate.UseTestDouble<FakePersonAssociationPublisherCheckSumPersister>().For<IPersonAssociationPublisherCheckSumPersister>();

			// Gamification
			isolate.UseTestDouble<FakePurgeSettingRepository>().For<IPurgeSettingRepository>();

			// Forecast
			isolate.UseTestDouble<ForecastProvider>().For<ForecastProvider>();
			isolate.UseTestDouble<SkillDayChangedEventBuilder>().For<SkillDayChangedEventBuilder>();
			isolate.UseTestDouble<ForecastDayModelMapper>().For<ForecastDayModelMapper>();
			isolate.UseTestDouble<FetchAndFillSkillDays>().For<IFetchAndFillSkillDays>();

			// AppInsights
			isolate.UseTestDouble<FakeApplicationInsights>().For<IApplicationInsights>();
			isolate.UseTestDouble<FakeApplicationInsightsConfigReader>().For<IApplicationInsightsConfigurationReader>();

			// licensing
			isolate.UseTestDouble<FakeLicenseRepository>().For<ILicenseRepository, ILicenseRepositoryForLicenseVerifier>();


			// Repositories
			if (QueryAllAttributes<ThrowIfRepositoriesAreUsedAttribute>().Any())
			{
				SetupThrowingTestDoubles.ForAllRepositories(isolate);
			}
			else
			{
				isolate.UseTestDouble<FakeTenantAuditRepository>().For<ITenantAuditRepository>();
				isolate.UseTestDouble<FakeSkillCombinationResourceReader>().For<ISkillCombinationResourceReader>();
				isolate.UseTestDouble<FakePersonRepository>().For<IPersonRepository, IProxyForId<IPerson>, IPersonLoadAllWithAssociation>();
				isolate.UseTestDouble<FakeMultisiteDayRepository>().For<IMultisiteDayRepository>();
				isolate.UseTestDouble<FakeBusinessUnitRepository>().For<IBusinessUnitRepository>();
				isolate.UseTestDouble<FakeApplicationRoleRepository>().For<IApplicationRoleRepository>();
				isolate.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
				isolate.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
				isolate.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
				isolate.UseTestDouble<FakeGroupPageRepository>().For<IGroupPageRepository>();
				isolate.UseTestDouble<FakeExternalLogOnRepository>().For<IExternalLogOnRepository>();
				isolate.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
				isolate.UseTestDouble<FakeActivityRepository>().For<IActivityRepository, IProxyForId<IActivity>>();
				isolate.UseTestDouble<FakePlanningPeriodRepository>().For<IPlanningPeriodRepository>();
				isolate.UseTestDouble<FakeExistingForecastRepository>().For<IExistingForecastRepository>();
				isolate.UseTestDouble<FakeDayOffTemplateRepository>().For<IDayOffTemplateRepository>();
				isolate.UseTestDouble<FakeRepositoryFactory>().For<IRepositoryFactory>();
				isolate.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
				isolate.UseTestDouble<FakeAbsenceRepository>().For<IAbsenceRepository>();
				isolate.UseTestDouble<FakeShiftCategoryRepository>().For<IShiftCategoryRepository>();
				isolate.UseTestDouble<FakeContractRepository>().For<IContractRepository>();
				isolate.UseTestDouble<FakeContractScheduleRepository>().For<IContractScheduleRepository>();
				isolate.UseTestDouble<FakeScheduleTagRepository>().For<IScheduleTagRepository>();
				isolate.UseTestDouble<FakeWorkflowControlSetRepository>().For<IWorkflowControlSetRepository>();
				isolate.UseTestDouble<FakeMeetingRepository>().For<IMeetingRepository>();
				isolate.UseTestDouble<FakeAgentDayScheduleTagRepository>().For<IAgentDayScheduleTagRepository>();
				isolate.UseTestDouble<FakePreferenceDayRepository>().For<IPreferenceDayRepository>();
				isolate.UseTestDouble<FakeStudentAvailabilityDayRepository>().For<IStudentAvailabilityDayRepository>();
				isolate.UseTestDouble<FakeOvertimeAvailabilityRepository>().For<IOvertimeAvailabilityRepository>();
				isolate.UseTestDouble<FakePersonAvailabilityRepository>().For<IPersonAvailabilityRepository>();
				isolate.UseTestDouble<FakePersonRotationRepository>().For<IPersonRotationRepository>();
				isolate.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
				isolate.UseTestDouble<FakePlanningGroupRepository>().For<IPlanningGroupRepository>();
				isolate.UseTestDouble<FakeStatisticRepository>().For<IStatisticRepository>();
				isolate.UseTestDouble<FakeRtaStateGroupRepository>().For<IRtaStateGroupRepository>();
				isolate.UseTestDouble<FakeRtaMapRepository>().For<IRtaMapRepository>();
				isolate.UseTestDouble<FakeRtaRuleRepository>().For<IRtaRuleRepository>();
				isolate.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
				isolate.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
				isolate.UseTestDouble<FakePartTimePercentageRepository>().For<IPartTimePercentageRepository>();
				isolate.UseTestDouble<FakeMultiplicatorDefinitionSetRepository>().For<IMultiplicatorDefinitionSetRepository>();
				isolate.UseTestDouble<FakeIntradayMonitorDataLoader>().For<IIntradayMonitorDataLoader>();
				isolate.UseTestDouble<FakeApplicationFunctionRepository>().For<IApplicationFunctionRepository>();
				isolate.UseTestDouble<FakeAvailableDataRepository>().For<IAvailableDataRepository>();
				isolate.UseTestDouble<FakeIntervalLengthFetcher>().For<IIntervalLengthFetcher>();
				isolate.UseTestDouble<FakeSkillGroupRepository>().For<ISkillGroupRepository>();
				isolate.UseTestDouble<FakeLoadAllSkillInIntradays>().For<ILoadAllSkillInIntradays>();
				isolate.UseTestDouble<FakeGroupingReadOnlyRepository>().For<IGroupingReadOnlyRepository>();
				isolate.UseTestDouble<FakeCommonAgentNameProvider>().For<ICommonAgentNameProvider>();
				isolate.UseTestDouble<FakePersonRequestRepository>().For<IPersonRequestRepository>();
				isolate.UseTestDouble<FakeQueuedAbsenceRequestRepository>().For<IQueuedAbsenceRequestRepository>();
				isolate.UseTestDouble<FakeRequestStrategySettingsReader>().For<IRequestStrategySettingsReader>();
				isolate.UseTestDouble<FakeLatestStatisticsIntervalIdLoader>().For<ILatestStatisticsIntervalIdLoader>();
				isolate.UseTestDouble<FakeRuleSetBagRepository>().For<IRuleSetBagRepository>();
				isolate.UseTestDouble<FakeIntradayQueueStatisticsLoader>().For<IIntradayQueueStatisticsLoader>();
				isolate.UseTestDouble<FakeNoteRepository>().For<INoteRepository>();
				isolate.UseTestDouble<FakePublicNoteRepository>().For<IPublicNoteRepository>();
				isolate.UseTestDouble<FakeWorkloadRepository>().For<IWorkloadRepository>();
				isolate.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>();
				isolate.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
				isolate.UseTestDouble<FakeJobResultRepository>().For<IJobResultRepository>();
				isolate.UseTestDouble<FakeJobStartTimeRepository>().For<IJobStartTimeRepository>();
				isolate.UseTestDouble<FakeGlobalSettingDataRepository>().For<IGlobalSettingDataRepository>();
				isolate.UseTestDouble<FakeAnalyticsAbsenceRepository>().For<IAnalyticsAbsenceRepository>();
				isolate.UseTestDouble<FakeAnalyticsActivityRepository>().For<IAnalyticsActivityRepository>();
				isolate.UseTestDouble<FakeAnalyticsBridgeGroupPagePersonRepository>().For<IAnalyticsBridgeGroupPagePersonRepository>();
				isolate.UseTestDouble<FakeAnalyticsBridgeTimeZoneRepository>().For<IAnalyticsBridgeTimeZoneRepository>();
				isolate.UseTestDouble<FakeAnalyticsBusinessUnitRepository>().For<IAnalyticsBusinessUnitRepository>();
				isolate.UseTestDouble<FakeAnalyticsDateRepository>().For<IAnalyticsDateRepository>();
				isolate.UseTestDouble<FakeAnalyticsDayOffRepository>().For<IAnalyticsDayOffRepository>();
				isolate.UseTestDouble<FakeAnalyticsForecastWorkloadRepository>().For<IAnalyticsForecastWorkloadRepository>();
				isolate.UseTestDouble<FakeAnalyticsGroupPageRepository>().For<IAnalyticsGroupPageRepository>();
				isolate.UseTestDouble<FakeAnalyticsHourlyAvailabilityRepository>().For<IAnalyticsHourlyAvailabilityRepository>();
				isolate.UseTestDouble<FakeAnalyticsIntervalRepository>().For<IAnalyticsIntervalRepository>();
				isolate.UseTestDouble<FakeAnalyticsOvertimeRepository>().For<IAnalyticsOvertimeRepository>();
				isolate.UseTestDouble<FakeAnalyticsPermissionExecutionRepository>().For<IAnalyticsPermissionExecutionRepository>();
				isolate.UseTestDouble<FakeAnalyticsPermissionRepository>().For<IAnalyticsPermissionRepository>();
				isolate.UseTestDouble<FakeAnalyticsPersonPeriodRepository>().For<IAnalyticsPersonPeriodRepository>();
				isolate.UseTestDouble<FakeAnalyticsPreferenceRepository>().For<IAnalyticsPreferenceRepository>();
				isolate.UseTestDouble<FakeAnalyticsRequestRepository>().For<IAnalyticsRequestRepository>();
				isolate.UseTestDouble<FakeAnalyticsScenarioRepository>().For<IAnalyticsScenarioRepository>();
				isolate.UseTestDouble<FakeAnalyticsScheduleRepository>().For<IAnalyticsScheduleRepository>();
				isolate.UseTestDouble<FakeAnalyticsShiftCategoryRepository>().For<IAnalyticsShiftCategoryRepository>();
				isolate.UseTestDouble<FakeAnalyticsSkillRepository>().For<IAnalyticsSkillRepository>();
				isolate.UseTestDouble<FakeAnalyticsTeamRepository>().For<IAnalyticsTeamRepository>();
				isolate.UseTestDouble<FakeAnalyticsSiteRepository>().For<IAnalyticsSiteRepository>();
				isolate.UseTestDouble<FakeAnalyticsTimeZoneRepository>().For<IAnalyticsTimeZoneRepository>();
				isolate.UseTestDouble<FakeAnalyticsWorkloadRepository>().For<IAnalyticsWorkloadRepository>();
				isolate.UseTestDouble<FakeUserDeviceRepository>().For<IUserDeviceRepository>();
				isolate.UseTestDouble<FakeScheduleAuditTrailReport>().For<IScheduleAuditTrailReport>();
				isolate.UseTestDouble<FakeBudgetGroupRepository>().For<IBudgetGroupRepository>();
				isolate.UseTestDouble<FakeBudgetDayRepository>().For<IBudgetDayRepository>();
				isolate.UseTestDouble<FakePersonFinderReadOnlyRepository>().For<IPersonFinderReadOnlyRepository>();
				isolate.UseTestDouble<FakeScheduleDayReadModelRepository>().For<IScheduleDayReadModelRepository>();
				isolate.UseTestDouble<FakeSeatBookingRepository>().For<ISeatBookingRepository>();
				isolate.UseTestDouble<FakeSeatMapRepository>().For<ISeatMapLocationRepository>();
				isolate.UseTestDouble<FakeSeatPlanRepository>().For<ISeatPlanRepository>();
				isolate.UseTestDouble<SeatPlanner>().For<ISeatPlanner>();
				isolate.UseTestDouble<SeatMapPersister>().For<ISeatMapPersister>();
				isolate.UseTestDouble<SeatPlanPersister>().For<ISeatPlanPersister>();
				isolate.UseTestDouble<SeatBookingRequestAssembler>().For<ISeatBookingRequestAssembler>();
				isolate.UseTestDouble<SeatFrequencyCalculator>().For<ISeatFrequencyCalculator>();
				isolate.UseTestDouble<FakeOptionalColumnRepository>().For<IOptionalColumnRepository>();
				isolate.UseTestDouble<FakeShiftCategorySelectionRepository>().For<Domain.InterfaceLegacy.Domain.IRepository<IShiftCategorySelection>>();
				isolate.UseTestDouble<FakeShiftCategoryUsageFinder>().For<IShiftCategoryUsageFinder>();
				isolate.UseTestDouble<FakeAgentBadgeWithRankTransactionRepository>().For<IAgentBadgeWithRankTransactionRepository>();
				isolate.UseTestDouble<FakeAgentBadgeTransactionRepository>().For<IAgentBadgeTransactionRepository>();
				isolate.UseTestDouble<FakeExternalPerformanceDataRepository>().For<IExternalPerformanceDataRepository>();
				isolate.UseTestDouble<FakeTeamGamificationSettingRepository>().For<ITeamGamificationSettingRepository>();
				isolate.UseTestDouble<FakeAgentBadgeRepository>().For<IAgentBadgeRepository>();
				isolate.UseTestDouble<FakePersonScheduleDayReadModelPersister>().For<IPersonScheduleDayReadModelPersister>();
				isolate.UseTestDouble<FakeGamificationSettingRepository>().For<IGamificationSettingRepository>();
				isolate.UseTestDouble<FakeForecastDayOverrideRepository>().For<IForecastDayOverrideRepository>();
				isolate.UseTestDouble<FakeExternalPerformanceRepository>().For<IExternalPerformanceRepository>();
				isolate.UseTestDouble<FakeExtensiveLogRepository>().For<IExtensiveLogRepository>();
				isolate.UseTestDouble<FakeStaffingAuditRepository>().For<IStaffingAuditRepository>();
				isolate.UseTestDouble<FakeASMScheduleChangeTimeRepository>().For<IASMScheduleChangeTimeRepository>();
				isolate.UseTestDouble<FakePayrollExportRepository>().For<IPayrollExportRepository>();
				isolate.UseTestDouble<FakePayrollResultRepository>().For<IPayrollResultRepository>();
				isolate.UseTestDouble<FakeSystemJobStartTimeRepository>().For<ISystemJobStartTimeRepository>();
				isolate.UseTestDouble<FakeSkillForecastReadModelRepository>().For<ISkillForecastReadModelRepository, FakeSkillForecastReadModelRepository>();
			}

			isolate.UseTestDouble<PersonSearchProvider>().For<PersonSearchProvider>();

			isolate.UseTestDouble<ScheduleStorageRepositoryWrapper>().For<IScheduleStorageRepositoryWrapper>();
			isolate.UseTestDouble<ReplaceLayerInSchedule>().For<IReplaceLayerInSchedule>();

			isolate.UseTestDouble<FakeFetchAllToggleOverrides>().For<IFetchAllToggleOverrides>();
			isolate.UseTestDouble<FakePersistToggleOverride>().For<IPersistToggleOverride>();

			if (QueryAllAttributes<LoggedOnAppDomainAttribute>().Any())
				isolate.UseTestDouble<FakeAppDomainPrincipalContext>().For<IThreadPrincipalContext>();
			else
				isolate.UseTestDouble<FakeThreadPrincipalContext>().For<IThreadPrincipalContext>();

			if (fullPermissions())
				isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
			if (fakePermissions())
				isolate.UseTestDouble<FakePermissions>().For<IAuthorization>();
		}

		public IPrincipalFactory PrincipalFactory;
		public IThreadPrincipalContext PrincipalContext;
		public FakeDataSourceForTenant DataSourceForTenant;
		public IDataSourceScope DataSourceScope;
		public IPersonRepository Persons;
		public Lazy<FakeDatabase> Database;
		public FakeEventPublisher FakeEventPublisher;

		private IDisposable _tenantScope;
		private Person _loggedOnPerson;

		protected override void BeforeTest()
		{
			base.BeforeTest();

			extendScope();

			var bu = new BusinessUnit("testbu").WithId(DefaultBusinessUnitId);
			fakeSignin(bu);

			createDefaultData(bu);
		}

		private void extendScope()
		{
			QueryAllAttributes<ExtendScopeAttribute>()
				.Select(x => x.Handler)
				.ForEach(x => FakeEventPublisher.AddHandler(x));
		}

		private void fakeSignin(IBusinessUnit businessUnit)
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

				var principal = PrincipalFactory.MakePrincipal(
					_loggedOnPerson,
					DataSourceForTenant.Tenant(DefaultTenantName),
					businessUnit,
					null
				);
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

		private void createDefaultData(IBusinessUnit businessUnit)
		{
			if (_loggedOnPerson != null)
				(Persons as FakePersonRepository)?.Has(_loggedOnPerson);

			if (QueryAllAttributes<DefaultDataAttribute>().Any() && !QueryAllAttributes<NoDefaultDataAttribute>().Any())
				Database.Value.CreateDefaultData(businessUnit);
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

			clearInstance();
		}

		private void clearInstance()
		{
			_tenantScope?.Dispose();
			_tenantScope = null;
			DataSourceForTenant = null;
			DataSourceScope = null;
			Database = null;
			Persons = null;
			FakeEventPublisher = null;
		}
	}
}
