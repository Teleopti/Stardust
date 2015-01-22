﻿using System;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Rta.WebService;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class RtaModule : Module
	{
		private readonly IIocConfiguration _config;

		public RtaModule(IIocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TeleoptiRtaService>().AsSelf().As<ITeleoptiRtaService>().SingleInstance();
			builder.RegisterType<Rta>().As<IRta>().SingleInstance();
			builder.RegisterType<AgentStateAssembler>().SingleInstance();
			builder.RegisterType<AlarmFinder>().As<IAlarmFinder>().SingleInstance();
			builder.RegisterType<RtaProcessor>().SingleInstance();
			builder.RegisterType<AgentStateReadModelUpdater>().As<IAgentStateReadModelUpdater>().SingleInstance();

			builder.RegisterType<DatabaseConnectionStringHandler>().As<IDatabaseConnectionStringHandler>();
			builder.RegisterType<DatabaseConnectionFactory>().As<IDatabaseConnectionFactory>();
			//mark activityalarms and stategroups to be cached
			_config.Args().CacheBuilder
				.For<DatabaseReader>()
				.CacheMethod(x => x.ActivityAlarms())
				.CacheMethod(x => x.StateGroups())
				.CacheMethod(x => x.GetCurrentSchedule(Guid.NewGuid()))
				.CacheMethod(x => x.Datasources())
				.CacheMethod(x => x.ExternalLogOns())
				.As<IDatabaseReader>();
			builder.RegisterMbCacheComponent<DatabaseReader, IDatabaseReader>();

			builder.Register<IReadActualAgentStates>(c => c.Resolve<DatabaseReader>());
			builder.RegisterType<DatabaseWriter>().As<IDatabaseWriter>().SingleInstance();

			if (_config.Toggle(Toggles.RTA_NoBroker_31237))
			{
				builder.RegisterType<NoMessagge>().As<IAgentStateMessageSender>().SingleInstance();
				builder.RegisterType<NoAggregation>().As<IAdherenceAggregator>().SingleInstance();
			}
			else
			{
				builder.RegisterType<AgentStateMessageSender>().As<IAgentStateMessageSender>().SingleInstance();
				builder.RegisterType<AdherenceAggregator>().As<IAdherenceAggregator>().SingleInstance();
			}

			builder.RegisterType<CalculateAdherence>().SingleInstance().As<ICalculateAdherence>();
			builder.RegisterType<CalculateAdherenceDetails>().SingleInstance().As<ICalculateAdherenceDetails>();

			if (_config.Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783) ||
				_config.Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285))
			{
				builder.RegisterType<ShiftEventPublisher>().SingleInstance().As<IShiftEventPublisher>();
				builder.RegisterType<AdherenceEventPublisher>().SingleInstance().As<IAdherenceEventPublisher>();
			}
			else
			{
				builder.RegisterType<NoEvents>().SingleInstance().As<IShiftEventPublisher>();
				builder.RegisterType<NoEvents>().SingleInstance().As<IAdherenceEventPublisher>();
			}

			if (_config.Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783) ||
				_config.Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285))
			{
				builder.RegisterType<StateEventPublisher>().SingleInstance().As<IStateEventPublisher>();
				builder.RegisterType<ActivityEventPublisher>().SingleInstance().As<IActivityEventPublisher>();
			}
			else
			{
				builder.RegisterType<NoEvents>().SingleInstance().As<IStateEventPublisher>();
				builder.RegisterType<NoEvents>().SingleInstance().As<IActivityEventPublisher>();
			}

			builder.RegisterType<OrganizationForPerson>().SingleInstance().As<IOrganizationForPerson>();

			if (_config.Toggle(Toggles.RTA_EventStreamInitialization_31237))
				builder.RegisterType<StateStreamSynchronizer>().As<IStateStreamSynchronizer>().SingleInstance();
			else
				builder.RegisterType<NoStateStreamSynchronizer>().As<IStateStreamSynchronizer>().SingleInstance();

			_config.Args().CacheBuilder
				.For<PersonOrganizationProvider>()
				.CacheMethod(svc => svc.PersonOrganizationData())
				.As<IPersonOrganizationProvider>();
			builder.RegisterMbCacheComponent<PersonOrganizationProvider, IPersonOrganizationProvider>().SingleInstance();

			//messy for now
			builder.Register(c => new PersonOrganizationReader(c.Resolve<INow>(), c.Resolve<IDatabaseConnectionStringHandler>().AppConnectionString()))
				.SingleInstance().As<IPersonOrganizationReader>();
		}
	}
}