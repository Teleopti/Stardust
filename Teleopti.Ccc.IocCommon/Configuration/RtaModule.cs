using System;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Aspects;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Rta;

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
			builder.RegisterType<ActivityChangeProcessor>().SingleInstance();
			builder.RegisterType<RtaInitializor>().SingleInstance();
			builder.RegisterType<RtaTenants>().SingleInstance();
			builder.RegisterType<CacheInvalidator>().As<ICacheInvalidator>().SingleInstance();
			builder.RegisterType<RtaProcessor>().SingleInstance();
			builder.RegisterType<AgentStateReadModelUpdater>().As<IAgentStateReadModelUpdater>().SingleInstance();

			if (_config.Toggle(Toggles.RTA_NewEventHangfireRTA_34333))
			{
				builder.RegisterType<NoMessage>().As<IAgentStateMessageSender>().SingleInstance();
				builder.RegisterType<NoAggregation>().As<IAdherenceAggregator>().SingleInstance();
			}
			else
			{
				builder.RegisterType<AgentStateMessageSender>().As<IAgentStateMessageSender>().SingleInstance();
				builder.RegisterType<AdherenceAggregator>().As<IAdherenceAggregator>().SingleInstance();
			}

			if (_config.Toggle(Toggles.RTA_DeletedPersons_36041))
				builder.RegisterType<LoadFromDatabase>().As<IPersonLoader>().SingleInstance();
			else
				builder.RegisterType<LoadByResolve>().As<IPersonLoader>().SingleInstance();
			builder.RegisterType<OrganizationForPerson>().SingleInstance().As<IOrganizationForPerson>();

			if (_config.Toggle(Toggles.RTA_NewEventHangfireRTA_34333))
				builder.RegisterType<StateStreamSynchronizer>().As<IStateStreamSynchronizer>().SingleInstance();
			else
				builder.RegisterType<NoStateStreamSynchronizer>().As<IStateStreamSynchronizer>().SingleInstance();

			builder.RegisterType<StateMapper>().As<IStateMapper>().SingleInstance();

			builder.RegisterType<ConnectionStrings>().As<IConnectionStrings>();
			
			_config.Cache().This<AlarmMappingLoader>((c, b) => b
				.CacheMethod(x => x.Load())
				.CacheKey(c.Resolve<CachePerDataSource>())
				);
			_config.Cache().This<StateMappingLoader>((c, b) => b
				.CacheMethod(x => x.Load())
				.CacheKey(c.Resolve<CachePerDataSource>())
				);
			builder.CacheByClassProxy<AlarmMappingLoader>().As<IAlarmMappingLoader>().ApplyAspects().SingleInstance();
			builder.CacheByClassProxy<StateMappingLoader>().As<IStateMappingLoader>().ApplyAspects().SingleInstance();

			builder.RegisterType<StateCodeAdder>().As<IStateCodeAdder>().SingleInstance().ApplyAspects();

			_config.Cache().This<IDatabaseLoader>((c, b) => b
				.CacheMethod(x => x.GetCurrentSchedule(Guid.NewGuid()))
				.CacheMethod(x => x.Datasources())
				.CacheMethod(x => x.ExternalLogOns())
				.CacheMethod(x => x.PersonOrganizationData())
				.CacheKey(c.Resolve<CachePerDataSource>())
				);
			builder.CacheByInterfaceProxy<DatabaseLoader, IDatabaseLoader>().SingleInstance();
			builder.RegisterType<DatabaseReader>().As<IDatabaseReader>().SingleInstance();

			_config.Cache().This<TenantLoader>(b => b
				.CacheMethod(x => x.TenantNameByKey(null))
				.CacheMethod(x => x.Authenticate(null))
				);
			builder.CacheByClassProxy<TenantLoader>().ApplyAspects().SingleInstance();

			builder.RegisterType<PreviousStateInfoLoader>().As<IPreviousStateInfoLoader>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStateReadModelReader>().As<IAgentStateReadModelReader>().SingleInstance().ApplyAspects();

			builder.RegisterType<AgentStateReadModelPersister>().As<IAgentStateReadModelPersister>().SingleInstance().ApplyAspects();

			builder.RegisterType<AdherencePercentageViewModelBuilder>().SingleInstance().As<IAdherencePercentageViewModelBuilder>();
			builder.RegisterType<AdherenceDetailsViewModelBuilder>().SingleInstance().As<IAdherenceDetailsViewModelBuilder>();
			builder.RegisterType<AgentStateViewModelBuilder>().SingleInstance().As<IAgentStateViewModelBuilder>();
			builder.RegisterType<RtaDecoratingEventPublisher>().As<IRtaDecoratingEventPublisher>().SingleInstance();

			if (_config.Toggle(Toggles.RTA_NewEventHangfireRTA_34333))
			{
				builder.RegisterType<ShiftEventPublisher>().SingleInstance().As<IShiftEventPublisher>();
				builder.RegisterType<AdherenceEventPublisher>().SingleInstance().As<IAdherenceEventPublisher>();
			}
			else
			{
				builder.RegisterType<NoEvents>().SingleInstance().As<IShiftEventPublisher>();
				builder.RegisterType<NoEvents>().SingleInstance().As<IAdherenceEventPublisher>();
			}

			if (_config.Toggle(Toggles.RTA_NewEventHangfireRTA_34333))
			{
				builder.RegisterType<StateEventPublisher>().SingleInstance().As<IStateEventPublisher>();
				builder.RegisterType<ActivityEventPublisher>().SingleInstance().As<IActivityEventPublisher>();
			}
			else
			{
				builder.RegisterType<NoEvents>().SingleInstance().As<IStateEventPublisher>();
				builder.RegisterType<NoEvents>().SingleInstance().As<IActivityEventPublisher>();
			}

			if (_config.Toggle(Toggles.RTA_CalculatePercentageInAgentTimezone_31236))
			{
				builder.RegisterType<BelongsToDateDecorator>().As<IRtaEventDecorator>().SingleInstance();
				builder.RegisterType<CurrentBelongsToDateFromPersonsCurrentTime>()
					.As<ICurrentBelongsToDate>()
					.SingleInstance();
			}
			else
			{
				builder.RegisterType<CurrentBelongsToDateFromUtcNow>()
					.As<ICurrentBelongsToDate>()
					.SingleInstance();
			}

			if (_config.Toggle(Toggles.RTA_NeutralAdherence_30930))
				builder.RegisterType<BySetting>().As<IAppliedAdherence>();
			else
				builder.RegisterType<ByStaffingEffect>().As<IAppliedAdherence>();

			if (_config.Toggle(Toggles.Wfm_RTA_ProperAlarm_34975))
				builder.RegisterType<IsAlarmStartTime>().As<IAppliedAlarmStartTime>();
			else
				builder.RegisterType<OldAlarmStartTime>().As<IAppliedAlarmStartTime>();
			
			if (_config.Toggle(Toggles.RTA_MultiTenancy_32539))
				builder.RegisterType<TenantAuthenticator>().As<IAuthenticator>().SingleInstance();
			else
				builder.RegisterType<ConfiguredKeyAuthenticator>().As<IAuthenticator>().SingleInstance();
			
			if (_config.Toggle(Toggles.RTA_MultiTenancy_32539))
				builder.RegisterType<TenantDataSourceScope>().As<IRtaDataSourceScope>().SingleInstance();
			else
				builder.RegisterType<ConnectionStringDataSourceScope>().As<IRtaDataSourceScope>().SingleInstance();
		}
	}
}