using System;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;
using Teleopti.Ccc.Domain.Common;
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
			builder.RegisterType<TenantsInitializedInRta>().SingleInstance();
			builder.RegisterType<CacheInvalidator>().As<ICacheInvalidator>().SingleInstance();
			builder.RegisterType<RtaProcessor>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStateReadModelUpdater>().As<IAgentStateReadModelUpdater>().SingleInstance();
			
			if (_config.Toggle(Toggles.RTA_ScaleOut_36979))
				builder.RegisterType<LoadAllFromDatabase>().As<IStateContextLoader>().SingleInstance();
			else if (_config.Toggle(Toggles.RTA_DeletedPersons_36041))
				builder.RegisterType<LoadPersonFromDatabase>().As<IStateContextLoader>().SingleInstance();
			else
				builder.RegisterType<LoadFromCache>().As<IStateContextLoader>().SingleInstance();

			builder.RegisterType<StateStreamSynchronizer>().SingleInstance();

			builder.RegisterType<StateMapper>().SingleInstance();

			builder.RegisterType<ConnectionStrings>().As<IConnectionStrings>();
			
			_config.Cache().This<RuleMappingLoader>((c, b) => b
				.CacheMethod(x => x.Load())
				.CacheKey(c.Resolve<CachePerDataSource>())
				);
			_config.Cache().This<StateMappingLoader>((c, b) => b
				.CacheMethod(x => x.Load())
				.CacheKey(c.Resolve<CachePerDataSource>())
				);
			builder.CacheByClassProxy<RuleMappingLoader>().As<IRuleMappingLoader>().ApplyAspects().SingleInstance();
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

			builder.RegisterType<ShiftEventPublisher>().SingleInstance().As<IShiftEventPublisher>();
			builder.RegisterType<AdherenceEventPublisher>().SingleInstance().As<IAdherenceEventPublisher>();
			builder.RegisterType<StateEventPublisher>().SingleInstance().As<IStateEventPublisher>();
			builder.RegisterType<ActivityEventPublisher>().SingleInstance().As<IActivityEventPublisher>();
			
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
				builder.RegisterType<BySetting>().As<IAppliedAdherence>().SingleInstance();
			else
				builder.RegisterType<ByStaffingEffect>().As<IAppliedAdherence>().SingleInstance();

			if (_config.Toggle(Toggles.Wfm_RTA_ProperAlarm_34975))
				builder.RegisterType<ProperAlarm>().As<IAppliedAlarm>().SingleInstance()
					.OnActivated(e => ServiceLocatorForEntity.SetInstanceFromContainer(e.Instance))
					.OnRelease(e => ServiceLocatorForEntity.SetInstanceFromContainer(null as IAppliedAlarm));
			else
				builder.RegisterType<AllRulesIsAlarm>().As<IAppliedAlarm>().SingleInstance()
					.OnActivated(e => ServiceLocatorForEntity.SetInstanceFromContainer(e.Instance))
					.OnRelease(e => ServiceLocatorForEntity.SetInstanceFromContainer(null as IAppliedAlarm));

		}
	}
}