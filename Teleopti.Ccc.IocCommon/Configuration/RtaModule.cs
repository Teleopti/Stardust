using System;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
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

			if (_config.Toggle(Toggles.RTA_Optimize_39667))
			{
				if (_config.Toggle(Toggles.RTA_RuleMappingOptimization_39812))
					builder.RegisterType<EventualStateCodeAdder>().As<IStateCodeAdder>().SingleInstance();
				else
					builder.RegisterType<ScaleOutStateCodeAdder>().As<IStateCodeAdder>().SingleInstance();

				if (_config.Toggle(Toggles.RTA_BatchConnectionOptimization_40116))
				{
					if (_config.Toggle(Toggles.RTA_BatchQueryOptimization_40169))
						if (_config.Toggle(Toggles.RTA_PersonOrganizationQueryOptimization_40261))
							if (_config.Toggle(Toggles.RTA_ScheduleQueryOptimization_40260))
								if (_config.Toggle(Toggles.RTA_ConnectionQueryOptimizeAllTheThings_40262))
									builder.RegisterType<ContextLoaderWithConnectionQueryOptimizeAllTheThings>().As<IContextLoader>().ApplyAspects().SingleInstance();
								else
									builder.RegisterType<ContextLoaderWithScheduleOptimization>().As<IContextLoader>().ApplyAspects().SingleInstance();
							else
								if (_config.Toggle(Toggles.RTA_ConnectionQueryOptimizeAllTheThings_40262))
									builder.RegisterType<ContextLoaderWithConnectionQueryOptimizeAllTheThings>().As<IContextLoader>().ApplyAspects().SingleInstance();
								else
									builder.RegisterType<ContextLoaderWithPersonOrganizationQueryOptimization>().As<IContextLoader>().ApplyAspects().SingleInstance();
						else
							builder.RegisterType<ContextLoaderWithBatchConnectionOptimization>().As<IContextLoader>().ApplyAspects().SingleInstance();
					else
						builder.RegisterType<ContextLoaderWithBatchConnectionOptimization>().As<IContextLoader>().ApplyAspects().SingleInstance();
				}
				else
					builder.RegisterType<ContextLoaderWithParalellBatch>().As<IContextLoader>().ApplyAspects().SingleInstance();
			}
			else
			{
				if (_config.Toggle(Toggles.RTA_RuleMappingOptimization_39812))
					builder.RegisterType<EventualStateCodeAdder>().As<IStateCodeAdder>().SingleInstance();
				else
					builder.RegisterType<StateCodeAdder>().As<IStateCodeAdder>().SingleInstance().ApplyAspects();
				builder.RegisterType<ContextLoader>().As<IContextLoader>().ApplyAspects().SingleInstance();
			}
			if (_config.Toggle(Toggles.RTA_ScheduleQueryOptimizationFilteredCache_40260))
				builder.RegisterType<Filtered>().As<IScheduleCacheStrategy>().SingleInstance();
			else
				builder.RegisterType<ThreeDays>().As<IScheduleCacheStrategy>().SingleInstance();

			_config.Cache().This<IDatabaseLoader>((c, b) => b
				.CacheMethod(x => x.Datasources())
				.CacheKey(c.Resolve<CachePerDataSource>())
				);
			builder.CacheByInterfaceProxy<DatabaseLoader, IDatabaseLoader>().SingleInstance();
			builder.RegisterType<DatabaseReader>()
				.As<IScheduleProjectionReadOnlyReader>()
				.As<IDatabaseReader>()
				.SingleInstance();
			if (_config.Toggle(Toggles.RTA_RuleMappingOptimization_39812))
				builder.RegisterType<MappingReadModelReader>().As<IMappingReader>().SingleInstance();
			else
				builder.RegisterType<MappingReader>().As<IMappingReader>().SingleInstance();

			_config.Cache().This<TenantLoader>(b => b
				.CacheMethod(x => x.TenantNameByKey(null))
				.CacheMethod(x => x.Authenticate(null))
				);
			builder.CacheByClassProxy<TenantLoader>().ApplyAspects().SingleInstance();

			builder.RegisterType<AgentStateReadModelReader>().As<IAgentStateReadModelReader>().SingleInstance().ApplyAspects();

			builder.RegisterType<KeyValueStorePersister>().As<IKeyValueStorePersister>().SingleInstance().ApplyAspects();
			if (_config.Toggle(Toggles.RTA_ScheduleQueryOptimization_40260))
				builder.RegisterType<AgentStatePersisterWithSchedules>().As<IAgentStatePersister>().SingleInstance().ApplyAspects();
			else
				builder.RegisterType<AgentStatePersister>().As<IAgentStatePersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStateReadModelPersister>().As<IAgentStateReadModelPersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<MappingReadModelPersister>().As<IMappingReadModelPersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStateReadModelUpdater>().As<IAgentStateReadModelUpdater>().SingleInstance().ApplyAspects();

			builder.RegisterType<AgentViewModelBuilder>().SingleInstance();
			builder.RegisterType<AgentStatesViewModelBuilder>().SingleInstance();
			builder.RegisterType<SkillViewModelBuilder>().SingleInstance();
			builder.RegisterType<AdherencePercentageViewModelBuilder>().SingleInstance().As<IAdherencePercentageViewModelBuilder>();
			builder.RegisterType<AdherenceDetailsViewModelBuilder>().SingleInstance().As<IAdherenceDetailsViewModelBuilder>();
			builder.RegisterType<RtaDecoratingEventPublisher>().As<IRtaDecoratingEventPublisher>().SingleInstance();

			builder.RegisterType<ShiftEventPublisher>().SingleInstance().As<IShiftEventPublisher>();
			builder.RegisterType<AdherenceEventPublisher>().SingleInstance().As<IAdherenceEventPublisher>();
			builder.RegisterType<StateEventPublisher>().SingleInstance().As<IStateEventPublisher>();
			builder.RegisterType<ActivityEventPublisher>().SingleInstance().As<IActivityEventPublisher>();

			builder.RegisterType<BelongsToDateDecorator>().As<IRtaEventDecorator>().SingleInstance();
			builder.RegisterType<CurrentBelongsToDate>().SingleInstance();
			builder.RegisterType<AppliedAdherence>().SingleInstance();

			builder.RegisterType<ProperAlarm>().SingleInstance();

			builder.RegisterType<SiteViewModelBuilder>().SingleInstance();
			builder.RegisterType<TeamViewModelBuilder>().SingleInstance();
			builder.RegisterType<NumberOfAgentsInSiteReader>().As<INumberOfAgentsInSiteReader>().SingleInstance();
			builder.RegisterType<NumberOfAgentsInTeamReader>().As<INumberOfAgentsInTeamReader>().SingleInstance();

			builder.RegisterType<ActivityChangeChecker>().SingleInstance();
			builder.RegisterType<LicensedActivityChangeChecker>().SingleInstance();
			builder.RegisterType<UnlicensedActivityChangeChecker>().SingleInstance();
			builder.Register<Func<ILicenseActivatorProvider, LicensedActivityChangeChecker, UnlicensedActivityChangeChecker, IActivityChangeChecker>>(c =>
				(license, licensed, unlicensed) => hasRtaLicense(license) ? (IActivityChangeChecker) licensed : unlicensed);
		}

		private static bool hasRtaLicense(ILicenseActivatorProvider license)
		{
			return license.Current().EnabledLicenseOptionPaths.Contains(DefinedLicenseOptionPaths.TeleoptiCccRealTimeAdherence);
		}
	}
}