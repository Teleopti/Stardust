﻿using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.RtaTool;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.ApplicationLayer.Skill;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.UnitOfWork;
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
			builder.RegisterType<StateQueue>().As<IStateQueueReader>().As<IStateQueueWriter>().SingleInstance().ApplyAspects();
			builder.RegisterType<StateQueueWorker>().SingleInstance().ApplyAspects();
			builder.RegisterType<StateQueueTenants>().SingleInstance();
			builder.RegisterType<StateQueueUtilities>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStateProcessor>().SingleInstance().ApplyAspects();
			builder.RegisterType<StateMapper>().SingleInstance().ApplyAspects();
			builder.RegisterType<ExternalLogonMapper>().SingleInstance().ApplyAspects();
			builder.RegisterType<ScheduleCache>().SingleInstance().ApplyAspects();

			if (_config.Toggle(Toggles.RTA_SpreadTransactionLocksStrategy_41823))
				builder.RegisterType<ContextLoaderWithSpreadTransactionLockStrategy>().As<IContextLoader>().ApplyAspects().SingleInstance();
			else
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

			builder.RegisterType<AgentViewModelBuilder>().SingleInstance();
			builder.RegisterType<AgentStatesViewModelBuilder>().SingleInstance();
			builder.RegisterType<PhoneStateViewModelBuilder>().SingleInstance();
			builder.RegisterType<SkillViewModelBuilder>().SingleInstance();
			builder.RegisterType<SiteCardViewModelBuilder>().SingleInstance();
			builder.RegisterType<TeamCardViewModelBuilder>().SingleInstance();

			builder.RegisterType<AdherencePercentageViewModelBuilder>().SingleInstance().As<IAdherencePercentageViewModelBuilder>();
			builder.RegisterType<HistoricalAdherenceViewModelBuilder>().SingleInstance();

			builder.RegisterType<HistoricalAdherenceReadModelReader>().As<IHistoricalAdherenceReadModelReader>();
			builder.RegisterType<HistoricalAdherenceReadModelPersister>().As<IHistoricalAdherenceReadModelPersister>();

			builder.RegisterType<HistoricalChangeReadModelReader>().As<IHistoricalChangeReadModelReader>();
			builder.RegisterType<HistoricalChangeReadModelPersister>().As<IHistoricalChangeReadModelPersister>();

			builder.RegisterType<ShiftEventPublisher>().SingleInstance();
			builder.RegisterType<AdherenceEventPublisher>().SingleInstance();
			builder.RegisterType<StateEventPublisher>().SingleInstance();
			builder.RegisterType<ActivityEventPublisher>().SingleInstance();
			builder.RegisterType<RuleEventPublisher>().SingleInstance();

			builder.RegisterType<CurrentBelongsToDate>().SingleInstance();

			builder.RegisterType<ProperAlarm>().SingleInstance();

			builder.RegisterType<OrganizationViewModelBuilder>().SingleInstance();
			builder.RegisterType<NumberOfAgentsInSiteReader>().As<INumberOfAgentsInSiteReader>().SingleInstance();
			builder.RegisterType<NumberOfAgentsInTeamReader>().As<INumberOfAgentsInTeamReader>().SingleInstance();
			builder.RegisterType<OrganizationReader>().As<IOrganizationReader>().SingleInstance();
			builder.RegisterType<HardcodedSkillGroupingPageId>().SingleInstance();

			builder.RegisterType<CurrentScheduleReadModelUpdater>().SingleInstance().ApplyAspects();
			builder.RegisterType<ActivityChangeChecker>().AsSelf().As<IActivityChangeCheckerFromScheduleChangeProcessor>().SingleInstance();

			builder.RegisterType<RtaToolViewModelBuilderFromAgentState>().As<IRtaToolViewModelBuilder>().SingleInstance();

			if (_config.Toggle(Toggles.RTA_RtaTracer_45597))
				builder.RegisterType<RtaTracer>().As<IRtaTracer>().SingleInstance();
			else
				builder.RegisterType<NoRtaTracer>().As<IRtaTracer>().SingleInstance();
			builder.RegisterType<RtaTracerReader>().As<IRtaTracerReader>().SingleInstance();
			builder.RegisterType<RtaTracerWriter>().As<IRtaTracerWriter>().SingleInstance();
		}

	}
	
}
