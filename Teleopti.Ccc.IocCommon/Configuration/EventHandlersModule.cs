﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer.ScheduleProjectionReadOnly;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.IocCommon.Toggle;
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
					t.IsEventHandler() &&
					t.EnabledByToggle(_config)
				)
				.As(t =>
					t.HandleInterfaces()
						.Where(x => x.Method?.EnabledByToggle(_config) ?? true)
						.Select(x => x.Type)
				)
				.AsSelf()
				.SingleInstance()
				// when will someone think this is an anti-pattern? ;)
				.Except<IntradayOptimizationEventRunInSyncInFatClientProcessHandler>(ct =>
				{
					ct.As<IHandleEvent<IntradayOptimizationWasOrdered>>()
						.AsSelf()
						.InstancePerLifetimeScope()
						.ApplyAspects();
				})
				.Except<WebIntradayOptimizationStardustHandler>(ct =>
				{
					ct.As<IHandleEvent<WebIntradayOptimizationStardustEvent>>()
						.AsSelf()
						.InstancePerLifetimeScope()
						.ApplyAspects();
				})
				.Except<WebScheduleStardustHandler>(ct =>
				{
					ct.As<IHandleEvent<WebScheduleStardustEvent>>()
						.AsSelf()
						.InstancePerLifetimeScope()
						.ApplyAspects();
				})
				.Except<WebDayoffOptimizationStardustHandler>(ct =>
				{
					ct.As<IHandleEvent<WebDayoffOptimizationStardustEvent>>()
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
				})
				.Except<SchedulingEventHandler>(ct =>
				{
					ct.As(typeof(IHandleEvent<SchedulingWasOrdered>))
						.AsSelf()
						.InstancePerLifetimeScope()
						.ApplyAspects();
				})
				.Except<DayOffOptimizationEventHandler>(ct =>
				{
					if (_config.Toggle(Toggles.ResourcePlanner_LessResourcesXXL_74915))
					{
						ct.As(typeof(IHandleEvent<DayOffOptimizationWasOrdered>))
							.AsSelf()
							.InstancePerLifetimeScope()
							.ApplyAspects();						
					}
				})
				.Except<DayOffOptimizationEventHandlerOLD>(ct =>
				{
					if (!_config.Toggle(Toggles.ResourcePlanner_LessResourcesXXL_74915))
					{
						ct.As(typeof(IHandleEvent<DayOffOptimizationWasOrdered>))
							.AsSelf()
							.InstancePerLifetimeScope()
							.ApplyAspects();						
					}
				})
				.ApplyAspects();

			builder.RegisterType<UnitOfWorkTransactionEventSyncronization>().As<IEventSyncronization>().SingleInstance();
			builder.RegisterType<BusinessRulesForPersonalAccountUpdate>().As<IBusinessRulesForPersonalAccountUpdate>().InstancePerDependency();
			builder.RegisterType<ProjectionChangedEventBuilder>().As<IProjectionChangedEventBuilder>().SingleInstance();
			builder.RegisterType<ScheduleDayReadModelsCreator>().As<IScheduleDayReadModelsCreator>().SingleInstance();
			builder.RegisterType<ScheduleDayReadModelPersister>().SingleInstance();
			builder.RegisterType<ScheduleProjectionReadOnlyChecker>().SingleInstance();
			builder.RegisterType<ScheduleChangesSubscriptionPublisher>().SingleInstance();
			builder.RegisterType<PersonScheduleDayReadModelsCreator>().As<IPersonScheduleDayReadModelsCreator>().SingleInstance();
			builder.RegisterType<ScheduleDayReadModelComparer>().As<IScheduleDayReadModelComparer>().SingleInstance();
			builder.RegisterType<PersonScheduleDayReadModelPersister>().As<IPersonScheduleDayReadModelPersister>().SingleInstance();
			builder.RegisterType<PersonScheduleDayReadModelUpdaterPersister>().SingleInstance();
			builder.RegisterType<PersonScheduleDayReadModelFinder>().As<IPersonScheduleDayReadModelFinder>().SingleInstance();
			builder.RegisterType<CommonAgentNameProvider>().As<ICommonAgentNameProvider>().SingleInstance();
			builder.RegisterType<TrackingMessageSender>().As<ITrackingMessageSender>().SingleInstance();
			builder.RegisterType<IntervalLengthFetcher>().As<IIntervalLengthFetcher>().SingleInstance();
			if (_config.Toggle(Toggles.ResourcePlanner_SpeedUpEvents_74996))
			{
				builder.CacheByClassProxy<AnalyticsAbsenceMapper>().SingleInstance();
				_config.Cache().This<AnalyticsAbsenceMapper>(b => b.CacheMethod(m => m.Map(Guid.Empty)));
				builder.CacheByClassProxy<FetchAnalyticsScenarios>().SingleInstance();
				_config.Cache().This<FetchAnalyticsScenarios>(b => b.CacheMethod(m => m.Execute()));
			}
			else
			{
				builder.RegisterType<AnalyticsAbsenceMapper>().SingleInstance();
				builder.RegisterType<FetchAnalyticsScenarios>().SingleInstance();
			}
			builder.RegisterType<AnalyticsFactScheduleTimeMapper>().As<IAnalyticsFactScheduleTimeMapper>().SingleInstance();				
			builder.RegisterType<AnalyticsFactScheduleDateMapper>().As<IAnalyticsFactScheduleDateMapper>().SingleInstance();
			builder.RegisterType<AnalyticsFactSchedulePersonMapper>().As<IAnalyticsFactSchedulePersonMapper>().SingleInstance();
			builder.RegisterType<AnalyticsFactScheduleMapper>().As<IAnalyticsFactScheduleMapper>().SingleInstance();
			builder.RegisterType<AnalyticsFactScheduleDayCountMapper>().As<IAnalyticsFactScheduleDayCountMapper>().SingleInstance();
			builder.RegisterType<AnalyticsScheduleRepository>().As<IAnalyticsScheduleRepository>().SingleInstance();
			builder.RegisterType<AnalyticsScenarioRepository>().As<IAnalyticsScenarioRepository>().SingleInstance();
			builder.RegisterType<AnalyticsAbsenceRepository>().As<IAnalyticsAbsenceRepository>().SingleInstance();
			builder.RegisterType<AnalyticsShiftCategoryRepository>().As<IAnalyticsShiftCategoryRepository>().SingleInstance();
			builder.RegisterType<IndexMaintenanceRepository>().As<IIndexMaintenanceRepository>().SingleInstance();

			builder.RegisterType<DoNotNotify>().As<INotificationValidationCheck>().SingleInstance();

			if (_config.Toggle(Toggles.ResourcePlanner_SpeedUpEvents_48769))
			{
				builder.RegisterType<UpdateFactSchedules>().As<IUpdateFactSchedules>().SingleInstance().ApplyAspects();	
			}
			else
			{
				builder.RegisterType<UpdateFactSchedulesOLD>().As<IUpdateFactSchedules>().SingleInstance().ApplyAspects();
			}

			builder.RegisterType<ScheduleProjectionReadOnlyPersister>()
				.As<IScheduleProjectionReadOnlyPersister>()
				.SingleInstance();

			builder.RegisterType<PersonPeriodTransformer>().As<IPersonPeriodTransformer>().SingleInstance();

			_config.Cache().This<IAnalyticsDateRepository>((c, b) => b.CacheMethod(x => x.Date(new DateTime())).CacheKey(c.Resolve<CachePerDataSource>()));
			if (_config.Toggle(Toggles.ETL_EventbasedDate_39562))
			{
				builder.RegisterType<AnalyticsPersonPeriodDateFixer>().As<IAnalyticsPersonPeriodDateFixer>().SingleInstance();
				builder.RegisterType<PersonPeriodFilterForDateCreation>().As<IPersonPeriodFilter>().SingleInstance();
				builder.CacheByInterfaceProxy<AnalyticsDateRepositoryWithCreation, IAnalyticsDateRepository>();
			}
			else
			{
				builder.RegisterType<AnalyticsPersonPeriodDateFixerWithoutDateCreation>().As<IAnalyticsPersonPeriodDateFixer>().SingleInstance();
				builder.RegisterType<PersonPeriodFilter>().As<IPersonPeriodFilter>().SingleInstance();
				builder.CacheByInterfaceProxy<AnalyticsDateRepository, IAnalyticsDateRepository>();
			}

			if (_config.Toggle(Toggles.ETL_EventbasedTimeZone_40870))
				builder.RegisterType<AnalyticsTimeZoneRepositoryWithCreation>().As<IAnalyticsTimeZoneRepository>().SingleInstance();
			else
				builder.RegisterType<AnalyticsTimeZoneRepository>().As<IAnalyticsTimeZoneRepository>().SingleInstance();

			if (_config.Toggle(Toggles.ResourcePlanner_SpeedUpEvents_75415))
			{
				builder.RegisterType<ResourcePlannerSpeedUpEvents75415On>().As<IResourcePlannerSpeedUpEvents75415>();
			}
			else
			{
				builder.RegisterType<ResourcePlannerSpeedUpEvents75415Off>().As<IResourcePlannerSpeedUpEvents75415>();
			}
		}

	}

	public static class EventHandlerTypeExtensions
	{
		public static bool IsEventHandler(this Type t)
		{
			if (!t.HandleInterfaces().Any())
				return false;

			var runOnHangfire = typeof(IRunOnHangfire).IsAssignableFrom(t);
			var runOnStardust = typeof(IRunOnStardust).IsAssignableFrom(t);
			var runInSync = typeof(IRunInSync).IsAssignableFrom(t);
			var runInSyncInFatClientProcess = typeof(IRunInSyncInFatClientProcess).IsAssignableFrom(t);
			if (!(runOnHangfire ^ runOnStardust ^ runInSync ^ runInSyncInFatClientProcess))
				throw new Exception($"All event handlers need to implement an IRunOn* interface. {t.Name} does not.");

			return true;
		}

		public static IEnumerable<HandlerInfo> HandleInterfaces(this Type t)
		{
			foreach (var i in t.GetInterfaces())
			{
				if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleEvent<>))
				{
					var eventType = i.GetMethods().Single().GetParameters().Single().ParameterType;
					yield return new HandlerInfo
					{
						Type = i,
						Method = t.GetMethod("Handle", new[] { eventType })
					};
				}
				if (i == typeof(IHandleEvents))
				{
					yield return new HandlerInfo
					{
						Type = i,
						Method = t.GetMethod("Handle")
					};
				}
			}
		}

		public class HandlerInfo
		{
			public Type Type { get; set; }
			public MethodInfo Method { get; set; }
		}
	}
}