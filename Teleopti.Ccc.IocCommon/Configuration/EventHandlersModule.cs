﻿using System;
using Autofac;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
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
			builder.RegisterEventHandlers(_config.Toggle, typeof(IHandleEvent<>).Assembly);

			builder.RegisterType<ReadModelValidator>().As<IReadModelValidator>().SingleInstance();
			builder.RegisterType<ReadModelFixer>().As<IReadModelFixer>().SingleInstance();
			builder.RegisterType<ReadModelPersonScheduleDayValidator>().As<IReadModelPersonScheduleDayValidator>();
			builder.RegisterType<ReadModelScheduleProjectionReadOnlyValidator>().As<IReadModelScheduleProjectionReadOnlyValidator>();
			builder.RegisterType<ReadModelScheduleDayValidator>().As<IReadModelScheduleDayValidator>();
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
			builder.CacheByClassProxy<AnalyticsAbsenceMapper>().SingleInstance();
			_config.Cache().This<AnalyticsAbsenceMapper>(b => b.CacheMethod(m => m.Map(Guid.Empty)));
			builder.CacheByClassProxy<FetchAnalyticsScenarios>().SingleInstance();
			_config.Cache().This<FetchAnalyticsScenarios>(b => b.CacheMethod(m => m.Execute()));
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
			
			builder.RegisterType<UpdateFactSchedules>().SingleInstance().ApplyAspects();	

			builder.RegisterType<ScheduleProjectionReadOnlyPersister>()
				.As<IScheduleProjectionReadOnlyPersister>()
				.SingleInstance();

			builder.RegisterType<PersonPeriodTransformer>().As<IPersonPeriodTransformer>().SingleInstance();

			_config.Cache().This<IAnalyticsDateRepository>((c, b) => b.CacheMethod(x => x.Date(new DateTime())).CacheKey(c.Resolve<CachePerDataSource>()));
			builder.RegisterType<AnalyticsPersonPeriodDateFixer>().As<IAnalyticsPersonPeriodDateFixer>().SingleInstance();
			builder.RegisterType<PersonPeriodFilterForDateCreation>().As<IPersonPeriodFilter>().SingleInstance();
			builder.CacheByInterfaceProxy<AnalyticsDateRepositoryWithCreation, IAnalyticsDateRepository>();
			builder.RegisterType<AnalyticsTimeZoneRepositoryWithCreation>().As<IAnalyticsTimeZoneRepository>().SingleInstance();
		}
	}
}