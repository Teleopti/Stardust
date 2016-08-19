﻿using System;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
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
using Teleopti.Ccc.Domain.ResourceCalculation;
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
                        let isHandlerEnabled = t.TypeEnabledByToggle(_config)
                        where isHandler && isHandlerEnabled
                        select i
                        ).ToArray();

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
                            throw new Exception(string.Format("All events handlers need to implement IRunOnHangfire or IRunOnServiceBus or IRunOnStardust or IRunInProcess. {0} does not.", t.Name));
                    }

                    return hasHandleInterfaces;
                })
                .As(t =>
                {
                    return
                        from i in t.GetInterfaces()

                        let isHandler = i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleEvent<>)
                        let eventType = isHandler ? i.GetMethods().Single().GetParameters().Single().ParameterType : null
                        let isHandleMethodEnabled = isHandler && t.GetMethod("Handle", new[] { eventType }).MethodEnabledByToggle(_config)

                        let isInitializable = i == typeof(IInitializeble)
                        let isSynchronizable = i == typeof(IRecreatable)
                        where
                            (isHandler && isHandleMethodEnabled) ||
                            isInitializable ||
                            isSynchronizable
                        select i;
                })
                .AsSelf()
                .SingleInstance()
                .ApplyAspects()
                .Except<IntradayOptimizationEventHandler>(ct =>
                {
                    ct.As<IHandleEvent<OptimizationWasOrdered>>()
                        .AsSelf()
                        .InstancePerLifetimeScope()
                        .ApplyAspects();
                }).Except<ShiftTradeRequestHandlerBus>(ct =>
                {
                    if (_config.Toggle(Toggles.ShiftTrade_ToHangfire_38181)) return;
                    ct.As(
                            typeof(IHandleEvent<NewShiftTradeRequestCreatedEvent>),
                            typeof(IHandleEvent<AcceptShiftTradeEvent>))
                            .AsSelf()
                            .InstancePerLifetimeScope()
                            .ApplyAspects();
                })
                .Except<ShiftTradeRequestHandlerHangfire>(ct =>
                {
                    if (!_config.Toggle(Toggles.ShiftTrade_ToHangfire_38181)) return;
                    ct.As(
                            typeof(IHandleEvent<NewShiftTradeRequestCreatedEvent>),
                            typeof(IHandleEvent<AcceptShiftTradeEvent>))
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

            builder.RegisterType<TeamOutOfAdherenceReadModelPersister>()
                .As<ITeamOutOfAdherenceReadModelPersister>()
                .SingleInstance();
	        builder.RegisterType<SiteOutOfAdherenceReadModelPersister>()
		        .As<ISiteOutOfAdherenceReadModelPersister>()
		        .SingleInstance();

	        if (_config.Toggle(Toggles.RTA_RemoveSiteTeamOutOfAdherenceReadModels_40069))
	        {
		        builder.RegisterType<SiteInAlarmFromAgentStatesReadModelReader>()
			        .As<ISiteOutOfAdherenceReadModelReader>()
			        .SingleInstance();
		        builder.RegisterType<TeamInAlarmFromAgentStatesReadModelReader>()
			        .As<ITeamOutOfAdherenceReadModelReader>()
			        .SingleInstance();
	        }
	        else
	        {
		        builder.RegisterType<SiteOutOfAdherenceReadModelPersister>()
			        .As<ISiteOutOfAdherenceReadModelReader>()
			        .SingleInstance();
		        builder.RegisterType<TeamOutOfAdherenceReadModelPersister>()
			        .As<ITeamOutOfAdherenceReadModelReader>()
			        .SingleInstance();
	        }

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

            _config.Cache().This<IAnalyticsDateRepository>(b => b
                .CacheMethod(x => x.Dates())
                );
            builder.CacheByInterfaceProxy<AnalyticsDateRepository, IAnalyticsDateRepository>();

            if (!_config.Toggle(Toggles.ETL_SpeedUpIntradayPreference_37124))
            {
                _config.Cache().This<IAnalyticsScheduleRepository>(b => b
                    .CacheMethod(x => x.ShiftLengths())
                    );
                builder.CacheByInterfaceProxy<AnalyticsScheduleRepository, IAnalyticsScheduleRepository>();
            }

            builder.RegisterType<PerformanceCounter>().As<IPerformanceCounter>().SingleInstance();
            builder.RegisterType<DeviceInfoProvider>().As<IDeviceInfoProvider>().SingleInstance();

	        builder.RegisterType<ScheduleProjectionReadOnlyPersister>()
		        .As<IScheduleProjectionReadOnlyPersister>()
		        .SingleInstance();
			  builder.RegisterType<ScheduleForecastSkillReadModelRepository>()
				  .As<IScheduleForecastSkillReadModelRepository>()
				  .SingleInstance();
		}
	}

}