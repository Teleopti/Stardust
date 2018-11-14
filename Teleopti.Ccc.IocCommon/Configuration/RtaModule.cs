using System;
using System.Collections.Generic;
using Autofac;
using Hangfire.Server;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Skill;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence;
using Teleopti.Wfm.Adherence.ApplicationLayer.Infrastructure;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;
using Teleopti.Wfm.Adherence.Domain;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Domain.ApprovePeriodAsInAdherence;
using Teleopti.Wfm.Adherence.Domain.Configuration;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Infrastructure;
using Teleopti.Wfm.Adherence.Domain.Infrastructure.Service;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Tool;
using Teleopti.Wfm.Adherence.Tracer;
using Teleopti.Wfm.Adherence.Tracer.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class RtaModule : Module
	{
		private readonly IocConfiguration _config;

		public RtaModule(IocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<Rta>().SingleInstance().ApplyAspects();

			builder.RegisterType<StateQueue>().As<IStateQueueReader>().As<IStateQueueWriter>().SingleInstance().ApplyAspects();
			builder.RegisterType<StateQueueWorker>().AsSelf().As<IBackgroundProcess>().SingleInstance().ApplyAspects();
			builder.RegisterType<StateQueueTenants>().SingleInstance();

			if (_config.Toggle(Toggles.RTA_StateQueueFloodPrevention_77710))
			{
				if (_config.Args().BehaviorTestServer)
				{
					builder.RegisterType<AlwaysHealthyChecker>().As<IStateQueueHealthChecker>().SingleInstance();
				}
				else
				{
					builder.RegisterType<StateQueueHealthChecker>().As<IStateQueueHealthChecker>().SingleInstance().ApplyAspects();
					builder.RegisterType<StateQueueHealthCheckerProcess>().As<IBackgroundProcess>().SingleInstance().ApplyAspects();
				}
			}
			else
			{
				builder.RegisterType<AlwaysHealthyChecker>().As<IStateQueueHealthChecker>().SingleInstance();
			}

			builder.RegisterType<StateQueueUtilities>().SingleInstance().ApplyAspects();

			builder.RegisterType<ActiveTenantsUpdater>().As<IBackgroundProcess>().SingleInstance();
			builder.RegisterType<ActiveTenants>().SingleInstance();
			builder.RegisterType<AgentStateProcessor>().SingleInstance().ApplyAspects();
			builder.RegisterType<StateMapper>().SingleInstance().ApplyAspects();
			builder.RegisterType<ExternalLogonMapper>().SingleInstance().ApplyAspects();
			builder.RegisterType<ScheduleCache>().SingleInstance().ApplyAspects();

			builder.RegisterType<ContextLoader>().As<IContextLoader>().ApplyAspects().SingleInstance();
			builder.RegisterType<DeadLockRetrier>().SingleInstance();
			builder.RegisterType<DeadLockVictimPriority>().SingleInstance();

			builder.RegisterType<DataSourceMapper>().SingleInstance().ApplyAspects();
			builder.RegisterType<DataSourceReader>().As<IDataSourceReader>().SingleInstance();
			builder.RegisterType<MappingReadModelReader>().As<IMappingReader>().SingleInstance();

			_config.Args().Cache.This<TenantLoader>(b => b
				.CacheMethod(x => x.TenantNameByKey(null))
				.CacheMethod(x => x.Authenticate(null))
			);
			builder.CacheByClassProxy<TenantLoader>().ApplyAspects().SingleInstance();

			builder.RegisterType<AgentStateReadModelPersister>().As<IAgentStateReadModelPersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStateReadModelReader>().As<IAgentStateReadModelReader>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStateReadModelQueryBuilderConfiguration>().SingleInstance();

			builder.RegisterType<HistoricalOverviewReadModelPersister>()
				.As<IHistoricalOverviewReadModelReader>()
				.As<IHistoricalOverviewReadModelPersister>()
				.SingleInstance().ApplyAspects();

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
			builder.RegisterType<HistoricalOverviewViewModelBuilder>().SingleInstance();
			builder.RegisterType<HistoricalAdherenceDate>().SingleInstance();

			builder.RegisterType<ApprovePeriodAsInAdherenceCommandHandler>().SingleInstance();
			builder.RegisterType<ApprovePeriodAsInAdherence>().SingleInstance();
			builder.RegisterType<RemoveApprovedPeriodCommandHandler>().SingleInstance();
			builder.RegisterType<RemoveApprovedPeriod>().SingleInstance();
			builder.RegisterType<BelongsToDateMapper>().SingleInstance();

			builder.RegisterType<RtaEventStore>()
				.As<IRtaEventStore>()
				.As<IRtaEventStoreReader>()
				.As<IRtaEventStoreTester>()
				.As<IRtaEventStoreUpgradeWriter>()
				.SingleInstance();
			if (_config.Toggle(Toggles.RTA_SpeedUpHistoricalAdherence_EventStoreUpgrader_78485))
			{
				builder.RegisterType<RtaEventStoreUpgrader>().As<IRtaEventStoreUpgrader>().SingleInstance().ApplyAspects();
				builder.RegisterType<RtaEventStoreUpgraderProcess>().As<IBackgroundProcess>().SingleInstance().ApplyAspects();
			}

			if (_config.Toggle(Toggles.RTA_ReviewHistoricalAdherence_74770))
			{
				builder.RegisterType<RtaEventStoreSynchronizer>().As<IRtaEventStoreSynchronizer>().SingleInstance().ApplyAspects();
				builder.RegisterType<RtaEventStoreSynchronizerWaiter>().As<IRtaEventStoreSynchronizerWaiter>().SingleInstance().ApplyAspects();
				builder.RegisterType<RtaEventStoreAsyncSynchronizer>().As<IRtaEventStoreAsyncSynchronizer>().SingleInstance().ApplyAspects();
				builder.RegisterType<RunAsynchronouslyAndLog>().As<IRtaEventStoreAsyncSynchronizerStrategy>().SingleInstance().ApplyAspects();
			}
			else
			{
				builder.RegisterType<DontSynchronize>()
					.As<IRtaEventStoreSynchronizer>()
					.As<IRtaEventStoreAsyncSynchronizer>()
					.As<IRtaEventStoreSynchronizerWaiter>()
					.SingleInstance();
			}

			if (_config.Toggle(Toggles.RTA_SpeedUpHistoricalAdherence_RemoveScheduleDependency_78485))
				builder.RegisterType<AgentAdherenceDayLoaderSpeedUpRemoveScheduleDependency>().As<IAgentAdherenceDayLoader>().SingleInstance();
			else if (_config.Toggle(Toggles.RTA_SpeedUpHistoricalAdherence_RemoveLastBefore_78306))
				builder.RegisterType<AgentAdherenceDayLoaderSpeedUpRemoveLastBefore>().As<IAgentAdherenceDayLoader>().SingleInstance();
			else if (_config.Toggle(Toggles.RTA_ReviewHistoricalAdherence_Domain_74770))
				builder.RegisterType<AgentAdherenceDayLoaderHistoricalOverview>().As<IAgentAdherenceDayLoader>().SingleInstance();
			else
				builder.RegisterType<AgentAdherenceDayLoaderDurationOfEvents>().As<IAgentAdherenceDayLoader>().SingleInstance();

			if (_config.Toggle(Toggles.RTA_ReviewHistoricalAdherence_Domain_74770))
				builder.RegisterType<ScheduleLoaderHistoricalOverview>().As<IScheduleLoader>().SingleInstance();
			else
				builder.RegisterType<ScheduleLoader>().As<IScheduleLoader>().SingleInstance();
			builder.RegisterType<AdherencePercentageCalculator>().SingleInstance();

			if (_config.Toggle(Toggles.RTA_SpeedUpHistoricalAdherence_RemoveLastBefore_78306))
				builder.RegisterType<AdherenceDayStartEventPublisher>().As<IAdherenceDayStartEventPublisher>().SingleInstance();
			else
				builder.RegisterType<NoAdherenceDayStartEventPublisher>().As<IAdherenceDayStartEventPublisher>().SingleInstance();

			builder.RegisterType<ShiftEventPublisher>().SingleInstance();
			builder.RegisterType<AdherenceEventPublisher>().SingleInstance();
			builder.RegisterType<StateEventPublisher>().SingleInstance();
			builder.RegisterType<ActivityEventPublisher>().SingleInstance();
			builder.RegisterType<RuleEventPublisher>().SingleInstance();
			builder.RegisterType<LateForWorkEventPublisher>().SingleInstance();

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

			builder.RegisterType<RtaTracerRefresher>().AsSelf().As<IBackgroundProcess>().SingleInstance().ApplyAspects();
			builder.RegisterType<RtaTracerReader>().As<IRtaTracerReader>().SingleInstance();
			builder.RegisterType<RtaTracerWriter>().As<IRtaTracerWriter>().SingleInstance();
			builder.RegisterType<RtaTracerConfigPersister>().As<IRtaTracerConfigPersister>().SingleInstance();
			builder.RegisterType<RtaTracerSessionFactory>().SingleInstance();

			builder.RegisterType<ConfigurationValidator>().SingleInstance();

			builder.RegisterType<TenantFromAuthenticationKey>().As<ITenantFinder>().SingleInstance();
		}
	}
}