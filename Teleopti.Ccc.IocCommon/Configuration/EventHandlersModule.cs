﻿using System;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer.ScheduleProjectionReadOnly;
using Teleopti.Ccc.Infrastructure.Intraday;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.Infrastructure.Rta.Persisters;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Module = Autofac.Module;

namespace Teleopti.Ccc.IocCommon.Configuration
{
    internal class EventHandlersModule : Module
    {
        private readonly IIocConfiguration _config;

        public EventHandlersModule(IIocConfiguration config)
        {
            _config = config;
        }

        protected override void Load(ContainerBuilder builder)
        {
			builder.RegisterType<ReadModelValidator>().As<IReadModelValidator>().SingleInstance();
	        builder.RegisterType<ReadModelFixer>().As<IReadModelFixer>().SingleInstance();
			builder.RegisterType<ReadModelPersonScheduleDayValidator>().As<IReadModelPersonScheduleDayValidator>();
			builder.RegisterType<ReadModelScheduleProjectionReadOnlyValidator>().As<IReadModelScheduleProjectionReadOnlyValidator>();
			builder.RegisterType<ReadModelScheduleDayValidator>().As<IReadModelScheduleDayValidator>();

			builder.RegisterAssemblyTypes(typeof(IHandleEvent<>).Assembly)
                .Where(t =>
                {
					var handleInterfaces = (
						from i in t.GetInterfaces()
						let isHandler = i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleEvent<>)
						where isHandler
						let isHandlerEnabled = t.TypeEnabledByToggle(_config)
						where isHandlerEnabled select i)
						.ToArray();

                    var hasHandleInterfaces = handleInterfaces.Any();

                    if (hasHandleInterfaces)
                    {
                        var runOnHangfire = typeof(IRunOnHangfire).IsAssignableFrom(t);
#pragma warning disable 618
                        var runOnServiceBus = typeof(IRunOnServiceBus).IsAssignableFrom(t);
#pragma warning restore 618
                        var runOnStardust = typeof(IRunOnStardust).IsAssignableFrom(t);
                        var runInProcess = typeof(IRunInProcess).IsAssignableFrom(t);
                        if (!(runOnHangfire ^ runOnServiceBus ^ runOnStardust ^ runInProcess))
                            throw new Exception($"All events handlers need to implement IRunOnHangfire or IRunOnServiceBus or IRunOnStardust or IRunInProcess. {t.Name} does not.");
                    }

                    return hasHandleInterfaces;
                })
                .As(t => from i in t.GetInterfaces()

	                let isHandler = i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleEvent<>)
	                let eventType = isHandler ? i.GetMethods().Single().GetParameters().Single().ParameterType : null
	                let isHandleMethodEnabled = isHandler && t.GetMethod("Handle", new[] { eventType }).MethodEnabledByToggle(_config)
	                let isInitializable = i == typeof(IInitializeble)
	                where (isHandler && isHandleMethodEnabled) || isInitializable
	                select i)
                .AsSelf()
                .SingleInstance()
                .ApplyAspects()
                .Except<IntradayOptimizationEventHandler>(ct =>
                {
                    ct.As<IHandleEvent<OptimizationWasOrdered>>()
                        .AsSelf()
                        .InstancePerLifetimeScope()
                        .ApplyAspects();
                })
                .Except<ShiftTradeRequestHandler>(ct =>
                {
                    ct.As(
                            typeof(IHandleEvent<NewShiftTradeRequestCreatedEvent>),
                            typeof(IHandleEvent<AcceptShiftTradeEvent>))
                            .AsSelf()
                            .InstancePerLifetimeScope()
                            .ApplyAspects();
                })
				.Except<MultiAbsenceRequestsHandler>(ct =>
				{
					ct.As(
							typeof(IHandleEvent<NewMultiAbsenceRequestsCreatedEvent>))
							.AsSelf()
							.InstancePerLifetimeScope()
							.ApplyAspects();
				});


            builder.RegisterType<UnitOfWorkTransactionEventSyncronization>().As<IEventSyncronization>().SingleInstance();
            builder.RegisterType<BusinessRulesForPersonalAccountUpdate>().As<IBusinessRulesForPersonalAccountUpdate>().InstancePerDependency();
            builder.RegisterType<ProjectionChangedEventBuilder>().As<IProjectionChangedEventBuilder>().SingleInstance();
            builder.RegisterType<ScheduleDayReadModelsCreator>().As<IScheduleDayReadModelsCreator>().SingleInstance();
            builder.RegisterType<PersonScheduleDayReadModelsCreator>().As<IPersonScheduleDayReadModelsCreator>().SingleInstance();
            builder.RegisterType<ScheduleDayReadModelComparer>().As<IScheduleDayReadModelComparer>().SingleInstance();
            builder.RegisterType<PersonScheduleDayReadModelPersister>().As<IPersonScheduleDayReadModelPersister>().SingleInstance();
            builder.RegisterType<PersonScheduleDayReadModelFinder>().As<IPersonScheduleDayReadModelFinder>().SingleInstance();
            builder.RegisterType<CommonAgentNameProvider>().As<ICommonAgentNameProvider>().SingleInstance();
            builder.RegisterType<TrackingMessageSender>().As<ITrackingMessageSender>().SingleInstance();
            builder.RegisterType<AdherencePercentageReadModelPersister>()
                .As<IAdherencePercentageReadModelPersister>()
                .As<IAdherencePercentageReadModelReader>()
                .SingleInstance();
            builder.RegisterType<AdherenceDetailsReadModelPersister>()
                .As<IAdherenceDetailsReadModelPersister>()
                .As<IAdherenceDetailsReadModelReader>()
                .SingleInstance();
			
			builder.RegisterType<SiteInAlarmReader>()
					.As<ISiteInAlarmReader>()
					.SingleInstance();
			builder.RegisterType<TeamInAlarmReader>()
				.As<ITeamInAlarmReader>()
				.SingleInstance();
			
			builder.RegisterType<IntervalLengthFetcher>().As<IIntervalLengthFetcher>().SingleInstance();
            builder.RegisterType<AnalyticsFactScheduleTimeHandler>().As<IAnalyticsFactScheduleTimeHandler>().SingleInstance();
            builder.RegisterType<AnalyticsFactScheduleDateHandler>().As<IAnalyticsFactScheduleDateHandler>().SingleInstance();
            builder.RegisterType<AnalyticsFactSchedulePersonHandler>().As<IAnalyticsFactSchedulePersonHandler>().SingleInstance();
            builder.RegisterType<AnalyticsFactScheduleHandler>().As<IAnalyticsFactScheduleHandler>().SingleInstance();
            builder.RegisterType<AnalyticsFactScheduleDayCountHandler>().As<IAnalyticsFactScheduleDayCountHandler>().SingleInstance();
            builder.RegisterType<AnalyticsScheduleRepository>().As<IAnalyticsScheduleRepository>().SingleInstance();
			builder.RegisterType<AnalyticsScenarioRepository>().As<IAnalyticsScenarioRepository>().SingleInstance();
			builder.RegisterType<AnalyticsAbsenceRepository>().As<IAnalyticsAbsenceRepository>().SingleInstance();
			builder.RegisterType<AnalyticsShiftCategoryRepository>().As<IAnalyticsShiftCategoryRepository>().SingleInstance();
			builder.RegisterType<IndexMaintenanceRepository>().As<IIndexMaintenanceRepository>().SingleInstance();

			builder.RegisterType<DoNotNotify>().As<INotificationValidationCheck>().SingleInstance();

            builder.RegisterType<PerformanceCounter>().As<IPerformanceCounter>().SingleInstance();
            builder.RegisterType<DeviceInfoProvider>().As<IDeviceInfoProvider>().SingleInstance();

	        builder.RegisterType<ScheduleProjectionReadOnlyPersister>()
		        .As<IScheduleProjectionReadOnlyPersister>()
		        .SingleInstance();
	      
			builder.RegisterType<ScheduleForecastSkillReadModelRepository>()
				  .As<IScheduleForecastSkillReadModelRepository>()
				  .SingleInstance();

	        builder.RegisterType<RequestStrategySettingsReader>().As<IRequestStrategySettingsReader>().SingleInstance();

			builder.RegisterType<PersonPeriodTransformer>().As<IPersonPeriodTransformer>().SingleInstance();

			

			_config.Cache().This<IAnalyticsDateRepository>(b => b.CacheMethod(x => x.Date(new DateTime())));
			if (_config.Toggle(Toggles.ETL_EventbasedDate_39562))
			{
				builder.RegisterType<AnalyticsPersonPeriodDateFixer>().As<IAnalyticsPersonPeriodDateFixer>().SingleInstance();
				builder.RegisterType<PersonPeriodFilterForDateCreation>().As<IPersonPeriodFilter>().SingleInstance();
				builder.CacheByInterfaceProxy<AnalyticsDateRepositoryWithCreation, IAnalyticsDateRepository>();
				builder.RegisterType<AnalyticsDateRepositoryWithCreation>().As<IAnalyticsDateRepository>().SingleInstance();
			}
			else
			{
				builder.RegisterType<AnalyticsPersonPeriodDateFixerWithoutDateCreation>().As<IAnalyticsPersonPeriodDateFixer>().SingleInstance();
				builder.RegisterType<PersonPeriodFilter>().As<IPersonPeriodFilter>().SingleInstance();
				builder.CacheByInterfaceProxy<AnalyticsDateRepository, IAnalyticsDateRepository>();
				builder.RegisterType<AnalyticsDateRepository>().As<IAnalyticsDateRepository>().SingleInstance();
			}

	        if (_config.Toggle(Toggles.ETL_EventbasedTimeZone_40870))
				builder.RegisterType<AnalyticsTimeZoneRepositoryWithCreation>().As<IAnalyticsTimeZoneRepository>().SingleInstance();
			else
				builder.RegisterType<AnalyticsTimeZoneRepository>().As<IAnalyticsTimeZoneRepository>().SingleInstance();
		}
	}

}