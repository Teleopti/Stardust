﻿using System;
using System.Runtime.Remoting.Messaging;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

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
			builder.RegisterType<Rta>().As<IRta>().SingleInstance();
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

			builder.RegisterType<OrganizationForPerson>().SingleInstance().As<IOrganizationForPerson>();

			if (_config.Toggle(Toggles.RTA_NewEventHangfireRTA_34333))
				builder.RegisterType<StateStreamSynchronizer>().As<IStateStreamSynchronizer>().SingleInstance();
			else
				builder.RegisterType<NoStateStreamSynchronizer>().As<IStateStreamSynchronizer>().SingleInstance();

			builder.RegisterType<StateMapper>().As<IStateMapper>().SingleInstance();

			builder.RegisterType<ConnectionStrings>().As<IConnectionStrings>();

			_config.Args().CacheBuilder
				.For<AlarmMappingLoader>()
				.CacheMethod(x => x.Load())
				.AsImplemented();
			_config.Args().CacheBuilder
				.For<StateMappingLoader>()
				.CacheMethod(x => x.Load())
				.AsImplemented();
			builder.RegisterConcreteMbCacheComponent<AlarmMappingLoader>().As<IAlarmMappingLoader>().ApplyAspects();
			builder.RegisterConcreteMbCacheComponent<StateMappingLoader>().As<IStateMappingLoader>().ApplyAspects();

			builder.RegisterType<StateCodeAdder>().As<IStateCodeAdder>().SingleInstance().ApplyAspects();

			_config.Args().CacheBuilder
				.For<ScheduleLoader>()
				.CacheMethod(x => x.GetCurrentSchedule(Guid.NewGuid()))
				.As<IScheduleLoader>();
			builder.RegisterMbCacheComponent<ScheduleLoader, IScheduleLoader>();

			_config.Args().CacheBuilder
				.For<DatabaseReader>()
				.CacheMethod(x => x.Datasources())
				.CacheMethod(x => x.ExternalLogOns())
				.As<IDatabaseReader>();
			builder.RegisterMbCacheComponent<DatabaseReader, IDatabaseReader>();

			builder.RegisterType<PreviousStateInfoLoader>().As<IPreviousStateInfoLoader>().SingleInstance().ApplyAspects();
			builder.RegisterType<AgentStateReadModelReader>().As<IAgentStateReadModelReader>().SingleInstance().ApplyAspects();
			builder.RegisterType<DatabaseWriter>().As<IDatabaseWriter>().SingleInstance().ApplyAspects();

			builder.RegisterType<AdherencePercentageViewModelBuilder>().SingleInstance().As<IAdherencePercentageViewModelBuilder>();
			builder.RegisterType<AdherenceDetailsViewModelBuilder>().SingleInstance().As<IAdherenceDetailsViewModelBuilder>();
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

			_config.Args().CacheBuilder
				.For<PersonOrganizationProvider>()
				.CacheMethod(svc => svc.PersonOrganizationData())
				.As<IPersonOrganizationProvider>();
			builder.RegisterMbCacheComponent<PersonOrganizationProvider, IPersonOrganizationProvider>().SingleInstance();

			builder.RegisterType<PersonOrganizationReader>().SingleInstance().As<IPersonOrganizationReader>();

			if (_config.Toggle(Toggles.RTA_MultiTenancy_32539))
				builder.RegisterType<RtaTenantAuthenticator>().As<IRtaAuthenticator>().SingleInstance();
			else
				builder.RegisterType<RtaAuthenticator>().As<IRtaAuthenticator>().SingleInstance();
		}
	}

}