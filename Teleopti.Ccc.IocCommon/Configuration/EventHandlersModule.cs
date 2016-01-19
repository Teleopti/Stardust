using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.Infrastructure.Rta.Persisters;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;
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
			builder.RegisterAssemblyTypes(typeof (IHandleEvent<>).Assembly)
				.Where(t =>
				{
					var matches = from i in t.GetInterfaces()
						let isHandler = i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IHandleEvent<>)
						let isHandlerEnabled = t.TypeEnabledByToggle(_config)
						where isHandler && isHandlerEnabled
						select i;
					if (t is AdherenceDetailsReadModelUpdater)
					{
						var x = 0;
					}
					return matches.Any();
				})
				.As(t =>
				{
					return from i in t.GetInterfaces()

						let isHandler = i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IHandleEvent<>)
						let eventType = isHandler ? i.GetMethods().Single().GetParameters().Single().ParameterType : null
						let isHandleMethodEnabled = isHandler && t.GetMethod("Handle", new[] {eventType}).MethodEnabledByToggle(_config)

						let isInitializable = i == typeof (IInitializeble)
						let isSynchronizable = i == typeof (IRecreatable)
						where
							(isHandler && isHandleMethodEnabled) ||
							isInitializable ||
							isSynchronizable
						select i;
				})
				.AsSelf() // for testing
				.SingleInstance()
				.ApplyAspects()
				;

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
				.As<ITeamOutOfAdherenceReadModelReader>()
				.SingleInstance();
			builder.RegisterType<SiteOutOfAdherenceReadModelPersister>()
				.As<ISiteOutOfAdherenceReadModelPersister>()
				.As<ISiteOutOfAdherenceReadModelReader>()
				.SingleInstance();

			builder.RegisterType<IntervalLengthFetcher>().As<IIntervalLengthFetcher>().SingleInstance();
			builder.RegisterType<AnalyticsFactScheduleTimeHandler>().As<IAnalyticsFactScheduleTimeHandler>().SingleInstance();
			builder.RegisterType<AnalyticsFactScheduleDateHandler>().As<IAnalyticsFactScheduleDateHandler>().SingleInstance();
			builder.RegisterType<AnalyticsFactSchedulePersonHandler>().As<IAnalyticsFactSchedulePersonHandler>().SingleInstance();
			builder.RegisterType<AnalyticsFactScheduleHandler>().As<IAnalyticsFactScheduleHandler>().SingleInstance();
			builder.RegisterType<AnalyticsFactScheduleDayCountHandler>().As<IAnalyticsFactScheduleDayCountHandler>().SingleInstance();
			builder.RegisterType<AnalyticsScheduleRepository>().As<IAnalyticsScheduleRepository>().SingleInstance();

			builder.RegisterType<DontNotifyRtaToCheckForActivityChange>().As<INotifyRtaToCheckForActivityChange>().SingleInstance();
			builder.RegisterType<DoNotNotify>().As<INotificationValidationCheck>().SingleInstance();

			_config.Cache().This<IAnalyticsScheduleRepository>(b => b
				.CacheMethod(x => x.Absences())
				.CacheMethod(x => x.Activities())
				.CacheMethod(x => x.Dates())
				.CacheMethod(x => x.Scenarios())
				.CacheMethod(x => x.ShiftCategories())
				.CacheMethod(x => x.Overtimes())
				.CacheMethod(x => x.ShiftLengths())
				);
			builder.CacheByInterfaceProxy<AnalyticsScheduleRepository, IAnalyticsScheduleRepository>();

			builder.RegisterType<PerformanceCounter>().As<IPerformanceCounter>().SingleInstance();
			builder.RegisterType<PersonSkillProvider>().As<IPersonSkillProvider>().InstancePerLifetimeScope();
		}
	}
}