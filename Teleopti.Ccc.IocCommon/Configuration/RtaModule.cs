using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.Rta.Persisters;

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
			builder.RegisterType<RtaInitializor>().SingleInstance();
			builder.RegisterType<TenantsInitializedInRta>().SingleInstance();
			builder.RegisterType<RtaProcessor>().SingleInstance().ApplyAspects();
			builder.RegisterType<StateMapper>().SingleInstance();
			builder.RegisterType<StateStreamSynchronizer>().SingleInstance();
			builder.RegisterType<EventualStateCodeAdder>()
				.As<IStateCodeAdder>()
				.SingleInstance();

			if (_config.Toggle(Toggles.RTA_FasterActivityCheck_41380))
				builder.RegisterType<ContextLoaderWithFasterActivityCheck>().As<IContextLoader>().ApplyAspects().SingleInstance();
			else
				builder.RegisterType<ContextLoader>().As<IContextLoader>().ApplyAspects().SingleInstance();
			builder.RegisterType<ThreeDays>().As<IScheduleCacheStrategy>().SingleInstance();

			_config.Cache().This<IDatabaseLoader>((c, b) => b
				.CacheMethod(x => x.Datasources())
				.CacheKey(c.Resolve<CachePerDataSource>())
				);
			builder.CacheByInterfaceProxy<DatabaseLoader, IDatabaseLoader>().SingleInstance();
			builder.RegisterType<DataSourceReader>().As<IDataSourceReader>().SingleInstance();
			if (_config.Toggle(Toggles.RTA_FasterUpdateOfScheduleChanges_40536))
				builder.RegisterType<FromPersonAssignment>().As<IScheduleReader>().SingleInstance().ApplyAspects();
			else
				builder.RegisterType<FromReadModel>().As<IScheduleReader>().SingleInstance().ApplyAspects();
			builder.RegisterType<MappingReadModelReader>().As<IMappingReader>().SingleInstance();

			_config.Cache().This<TenantLoader>(b => b
				.CacheMethod(x => x.TenantNameByKey(null))
				.CacheMethod(x => x.Authenticate(null))
				);
			builder.CacheByClassProxy<TenantLoader>().ApplyAspects().SingleInstance();

			builder.RegisterType<AgentStateReadModelReader>().As<IAgentStateReadModelReader>().SingleInstance().ApplyAspects();

			builder.RegisterType<KeyValueStorePersister>().As<IKeyValueStorePersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStatePersister>().As<IAgentStatePersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStateReadModelPersister>().As<IAgentStateReadModelPersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<MappingReadModelPersister>().As<IMappingReadModelPersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStateReadModelUpdater>().As<IAgentStateReadModelUpdater>().SingleInstance().ApplyAspects();

			builder.RegisterType<AgentViewModelBuilder>().SingleInstance();
			builder.RegisterType<AgentStatesViewModelBuilder>().SingleInstance();
			builder.RegisterType<PhoneStateViewModelBuilder>().SingleInstance();
			builder.RegisterType<SkillViewModelBuilder>().SingleInstance();
			builder.RegisterType<AgentsInAlarmForSiteViewModelBuilder>().SingleInstance();
			builder.RegisterType<AgentsInAlarmForTeamsViewModelBuilder>().SingleInstance();

			builder.RegisterType<AdherencePercentageViewModelBuilder>().SingleInstance().As<IAdherencePercentageViewModelBuilder>();
			builder.RegisterType<AdherenceDetailsViewModelBuilder>().SingleInstance().As<IAdherenceDetailsViewModelBuilder>();
			builder.RegisterType<HistoricalAdherenceViewModelBuilder>().SingleInstance();

			builder.RegisterType<HistoricalAdherenceReadModelReader>().As<IHistoricalAdherenceReadModelReader>();
			builder.RegisterType<HistoricalAdherenceReadModelPersister>().As<IHistoricalAdherenceReadModelPersister>();

			builder.RegisterType<ShiftEventPublisher>().SingleInstance();
			builder.RegisterType<AdherenceEventPublisher>().SingleInstance();
			builder.RegisterType<StateEventPublisher>().SingleInstance();
			builder.RegisterType<ActivityEventPublisher>().SingleInstance();

			builder.RegisterType<CurrentBelongsToDate>().SingleInstance();

			builder.RegisterType<ProperAlarm>().SingleInstance();

			builder.RegisterType<SiteViewModelBuilder>().SingleInstance();
			builder.RegisterType<TeamViewModelBuilder>().SingleInstance();
			builder.RegisterType<NumberOfAgentsInSiteReader>().As<INumberOfAgentsInSiteReader>().SingleInstance();
			builder.RegisterType<NumberOfAgentsInTeamReader>().As<INumberOfAgentsInTeamReader>().SingleInstance();
			builder.RegisterType<HardcodedSkillGroupingPageId>().SingleInstance();

			builder.RegisterType<ActivityChangeChecker>().SingleInstance();
			
			if (string.IsNullOrEmpty(_config.Args().RtaAgentStateTraceMatch))
				builder.RegisterType<DisabledTracer>().As<IAgentStateTracer>().SingleInstance();
			else
				builder.RegisterType<AgentStateTracer>().As<IAgentStateTracer>().SingleInstance();
		}

	}
}
