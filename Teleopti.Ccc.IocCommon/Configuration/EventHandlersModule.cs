using System.Linq;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
			builder.RegisterAssemblyTypes(typeof (IHandleEvent<>).Assembly)
				.Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IHandleEvent<>)))
				.As(t => t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IHandleEvent<>)))
				.Where(t => t.EnabledByToggle(_config))
				.SingleInstance()
				.EnableClassInterceptors().InterceptedBy(typeof (AspectInterceptor));
			
			builder.RegisterType<UnitOfWorkTransactionEventSyncronization>().As<IEventSyncronization>().SingleInstance();

			builder.RegisterType<ProjectionChangedEventBuilder>().As<IProjectionChangedEventBuilder>().SingleInstance();
			builder.RegisterType<ScheduleDayReadModelsCreator>().As<IScheduleDayReadModelsCreator>().SingleInstance();
			builder.RegisterType<PersonScheduleDayReadModelsCreator>().As<IPersonScheduleDayReadModelsCreator>().SingleInstance();
			builder.RegisterType<ScheduleDayReadModelComparer>().As<IScheduleDayReadModelComparer>().SingleInstance();
			builder.RegisterType<UpdateScheduleProjectionReadModel>().As<IUpdateScheduleProjectionReadModel>().SingleInstance();
			builder.RegisterType<PersonScheduleDayReadModelPersister>().As<IPersonScheduleDayReadModelPersister>().SingleInstance();
			builder.RegisterType<PersonScheduleDayReadModelFinder>().As<IPersonScheduleDayReadModelFinder>().SingleInstance();
			builder.RegisterType<CommonAgentNameProvider>().As<ICommonAgentNameProvider>().SingleInstance();
			builder.RegisterType<TrackingMessageSender>().As<ITrackingMessageSender>().SingleInstance();
			builder.RegisterType<AdherencePercentageReadModelPersister>().SingleInstance().As<IAdherencePercentageReadModelPersister>();
			builder.RegisterType<AdherenceDetailsReadModelPersister>().SingleInstance().As<IAdherenceDetailsReadModelPersister>();
			builder.RegisterType<IntervalLengthFetcher>().SingleInstance().As<IIntervalLengthFetcher>();
			builder.RegisterType<AnalyticsFactScheduleTimeHandler>().SingleInstance().As<IAnalyticsFactScheduleTimeHandler>();
			builder.RegisterType<AnalyticsFactScheduleDateHandler>().SingleInstance().As<IAnalyticsFactScheduleDateHandler>();
			builder.RegisterType<AnalyticsFactSchedulePersonHandler>().SingleInstance().As<IAnalyticsFactSchedulePersonHandler>();
			builder.RegisterType<AnalyticsScheduleRepository>().SingleInstance().As<IAnalyticsScheduleRepository>();

			builder.RegisterType<DontNotifyRtaToCheckForActivityChange>().As<INotifyRtaToCheckForActivityChange>().SingleInstance();
			builder.RegisterType<DoNotNotify>().As<INotificationValidationCheck>().SingleInstance();

			_config.Args().CacheBuilder
				.For<AnalyticsScheduleRepository>()
				.CacheMethod(x => x.Absences())
				.CacheMethod(x => x.Activities())
				.CacheMethod(x => x.LoadDimDates())
				.CacheMethod(x => x.Scenarios())
				.CacheMethod(x => x.ShiftCategories())
				.As<IAnalyticsScheduleRepository>();
			builder.RegisterMbCacheComponent<AnalyticsScheduleRepository, IAnalyticsScheduleRepository>();
		}
	}
}