using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Skill;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Configuration;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Domain.RealTimeAdherence.Tool;
using Teleopti.Ccc.Domain.RealTimeAdherence.Tracer;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.RealTimeAdherence.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Infrastructure.RealTimeAdherence.Tracer;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class RtaModule : Module
	{
		private readonly IIocConfiguration _config;

		public RtaModule(IIocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<Rta>().SingleInstance().ApplyAspects();
			builder.RegisterType<StateQueue>().As<IStateQueueReader>().As<IStateQueueWriter>().SingleInstance().ApplyAspects();
			builder.RegisterType<StateQueueWorker>().SingleInstance().ApplyAspects();
			builder.RegisterType<StateQueueTenants>().SingleInstance();
			builder.RegisterType<StateQueueUtilities>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStateProcessor>().SingleInstance().ApplyAspects();
			builder.RegisterType<StateMapper>().SingleInstance().ApplyAspects();
			builder.RegisterType<ExternalLogonMapper>().SingleInstance().ApplyAspects();
			builder.RegisterType<ScheduleCache>().SingleInstance().ApplyAspects();

			builder.RegisterType<ContextLoader>().As<IContextLoader>().ApplyAspects().SingleInstance();
			builder.RegisterType<DeadLockRetrier>().SingleInstance();
			builder.RegisterType<DeadLockVictimThrower>().SingleInstance();

			builder.RegisterType<DataSourceMapper>().SingleInstance().ApplyAspects();
			builder.RegisterType<DataSourceReader>().As<IDataSourceReader>().SingleInstance();
			builder.RegisterType<MappingReadModelReader>().As<IMappingReader>().SingleInstance();

			_config.Cache().This<TenantLoader>(b => b
				.CacheMethod(x => x.TenantNameByKey(null))
				.CacheMethod(x => x.Authenticate(null))
			);
			builder.CacheByClassProxy<TenantLoader>().ApplyAspects().SingleInstance();

			builder.RegisterType<AgentStateReadModelPersister>().As<IAgentStateReadModelPersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStateReadModelReader>().As<IAgentStateReadModelReader>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStateReadModelQueryBuilderConfiguration>().SingleInstance();

			builder.RegisterType<KeyValueStorePersister>().As<IKeyValueStorePersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStatePersister>().As<IAgentStatePersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<MappingReadModelPersister>().As<IMappingReadModelPersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<CurrentScheduleReadModelPersister>()
				.As<ICurrentScheduleReadModelPersister>()
				.As<IScheduleReader>()
				.SingleInstance().ApplyAspects();
			builder.RegisterType<ExternalLogonReadModelPersister>()
				.As<IExternalLogonReadModelPersister>()
				.As<IExternalLogonReader>()
				.SingleInstance().ApplyAspects();

			builder.RegisterType<AgentStatesViewModelBuilder>().SingleInstance();
			builder.RegisterType<PhoneStateViewModelBuilder>().SingleInstance();
			builder.RegisterType<SkillViewModelBuilder>().SingleInstance();
			builder.RegisterType<SiteCardViewModelBuilder>().SingleInstance();
			builder.RegisterType<TeamCardReader>().As<ITeamCardReader>().SingleInstance();
			builder.RegisterType<RtaTracerViewModelBuilder>().SingleInstance();
			builder.RegisterType<HistoricalAdherenceViewModelBuilder>().SingleInstance();
			builder.RegisterType<HistoricalAdherenceDate>().SingleInstance();

			builder.RegisterType<HistoricalChange>().SingleInstance();
			builder.RegisterType<ApprovePeriodAsInAdherenceCommandHandler>().SingleInstance();
			builder.RegisterType<ApprovePeriodAsInAdherence>().SingleInstance();
			builder.RegisterType<AgentAdherenceDayLoader>().SingleInstance();
			builder.RegisterType<ScheduleLoader>().SingleInstance();
			builder.RegisterType<AdherencePercentageCalculator>().SingleInstance();

			builder.RegisterType<HistoricalChangeReadModelReader>().As<IHistoricalChangeReadModelReader>().SingleInstance();
			builder.RegisterType<HistoricalChangeReadModelPersister>().As<IHistoricalChangeReadModelPersister>().SingleInstance();
			builder.RegisterType<ApprovedPeriodsReader>().As<IApprovedPeriodsReader>().SingleInstance();
			builder.RegisterType<ApprovedPeriodsPersister>().As<IApprovedPeriodsPersister>().SingleInstance();

			builder.RegisterType<ShiftEventPublisher>().SingleInstance();
			builder.RegisterType<AdherenceEventPublisher>().SingleInstance();
			builder.RegisterType<StateEventPublisher>().SingleInstance();
			builder.RegisterType<ActivityEventPublisher>().SingleInstance();
			builder.RegisterType<RuleEventPublisher>().SingleInstance();

			builder.RegisterType<CurrentBelongsToDate>().SingleInstance();

			builder.RegisterType<ProperAlarm>().SingleInstance();

			builder.RegisterType<OrganizationViewModelBuilder>().SingleInstance();
			builder.RegisterType<OrganizationReader>().As<IOrganizationReader>().SingleInstance();
			builder.RegisterType<HardcodedSkillGroupingPageId>().SingleInstance();

			builder.RegisterType<CurrentScheduleReadModelUpdater>().SingleInstance().ApplyAspects();
			builder.RegisterType<ActivityChangeChecker>().AsSelf().As<IActivityChangeCheckerFromScheduleChangeProcessor>().SingleInstance();

			builder.RegisterType<RtaToolViewModelBuilderFromAgentState>().SingleInstance();

			if (_config.Args().ConfigReader.ReadValue("UseSafeRtaTracer", true))
			{
				builder.RegisterType<RtaTracer>().SingleInstance().ApplyAspects();
				builder.RegisterType<SafeRtaTracer>().As<IRtaTracer>().SingleInstance();
			}
			else
			{
				builder.RegisterType<RtaTracer>().As<IRtaTracer>().SingleInstance().ApplyAspects();
			}

			builder.RegisterType<RtaTracerRefresher>().SingleInstance().ApplyAspects();
			builder.RegisterType<RtaTracerReader>().As<IRtaTracerReader>().SingleInstance();
			builder.RegisterType<RtaTracerWriter>().As<IRtaTracerWriter>().SingleInstance();
			builder.RegisterType<RtaTracerConfigPersister>().As<IRtaTracerConfigPersister>().SingleInstance();
			builder.RegisterType<RtaTracerSessionFactory>().SingleInstance();

			if (_config.Toggle(Toggles.RTA_ConfigurationValidationNotification_46933))
				builder.RegisterType<ConfigurationValidator>().As<IConfigurationValidator>().SingleInstance();
			else
				builder.RegisterType<NoConfigurationValidator>().As<IConfigurationValidator>().SingleInstance();
		}
	}
}