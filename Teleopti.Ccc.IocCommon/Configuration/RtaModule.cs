﻿using System;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.Aop;
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
			builder.RegisterType<AgentStateAssembler>().SingleInstance();
			builder.RegisterType<StateMapper>().As<IStateMapper>().SingleInstance();

			builder.RegisterType<DatabaseConnectionStringHandler>().As<IDatabaseConnectionStringHandler>();
			builder.RegisterType<DatabaseConnectionFactory>().As<IDatabaseConnectionFactory>();

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
				.For<DatabaseReader>()
				.CacheMethod(x => x.GetCurrentSchedule(Guid.NewGuid()))
				.CacheMethod(x => x.Datasources())
				.CacheMethod(x => x.ExternalLogOns())
				.As<IDatabaseReader>();
			builder.RegisterMbCacheComponent<DatabaseReader, IDatabaseReader>();

			builder.Register<IReadActualAgentStates>(c => c.Resolve<DatabaseReader>());
			builder.RegisterType<DatabaseWriter>().As<IDatabaseWriter>().SingleInstance();

			builder.RegisterType<AgentDateProvider>().SingleInstance().As<IAgentDateProvider>();
			builder.RegisterType<AdherencePercentageViewModelBuilder>().SingleInstance().As<IAdherencePercentageViewModelBuilder>();
			builder.RegisterType<AdherenceDetailsViewModelBuilder>().SingleInstance().As<IAdherenceDetailsViewModelBuilder>();
			builder.RegisterType<RtaDecoratingEventPublisher>().As<IRtaDecoratingEventPublisher>().SingleInstance();

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
				builder.RegisterType<ByPolicy>().As<IAppliedAdherence>();
			else
				builder.RegisterType<ByStaffingEffect>().As<IAppliedAdherence>();
			
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