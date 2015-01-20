using System.Configuration;
using System.Linq;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

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
						let toggleEnabled = t.EnabledByToggle(_config)
						where isHandler && toggleEnabled
						select i;
					return matches.Any();
				})
				.As(t =>
				{
					return from i in t.GetInterfaces()
						let isHandler = i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IHandleEvent<>)
						let isInitializable = i == typeof (IInitializeble)
						let isSynchronizable = i == typeof (IRecreatable)
						where isHandler || isInitializable || isSynchronizable
						select i;
				})
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
			builder.RegisterType<TeamOutOfAdherenceReadModelPersister>().SingleInstance().As<ITeamOutOfAdherenceReadModelPersister>();
			builder.RegisterType<SiteOutOfAdherenceReadModelPersister>().SingleInstance().As<ISiteOutOfAdherenceReadModelPersister>();
			builder.RegisterType<IntervalLengthFetcher>().SingleInstance().As<IIntervalLengthFetcher>();
			builder.RegisterType<AnalyticsFactScheduleTimeHandler>().SingleInstance().As<IAnalyticsFactScheduleTimeHandler>();
			builder.RegisterType<AnalyticsFactScheduleDateHandler>().SingleInstance().As<IAnalyticsFactScheduleDateHandler>();
			builder.RegisterType<AnalyticsFactSchedulePersonHandler>().SingleInstance().As<IAnalyticsFactSchedulePersonHandler>();
			builder.RegisterType<AnalyticsFactScheduleHandler>().SingleInstance().As<IAnalyticsFactScheduleHandler>();
			builder.RegisterType<AnalyticsFactScheduleDayCountHandler>().SingleInstance().As<IAnalyticsFactScheduleDayCountHandler>();
			builder.RegisterType<AnalyticsScheduleRepository>().SingleInstance().As<IAnalyticsScheduleRepository>();

			builder.RegisterType<DontNotifyRtaToCheckForActivityChange>().As<INotifyRtaToCheckForActivityChange>().SingleInstance();
			builder.RegisterType<DoNotNotify>().As<INotificationValidationCheck>().SingleInstance();

			_config.Args().CacheBuilder
				.For<AnalyticsScheduleRepository>()
				.CacheMethod(x => x.Absences())
				.CacheMethod(x => x.Activities())
				.CacheMethod(x => x.Dates())
				.CacheMethod(x => x.Scenarios())
				.CacheMethod(x => x.ShiftCategories())
				.CacheMethod(x => x.Overtimes())
				.CacheMethod(x => x.ShiftCategories())
				.As<IAnalyticsScheduleRepository>();
			builder.RegisterMbCacheComponent<AnalyticsScheduleRepository, IAnalyticsScheduleRepository>();

			// ErikS: Bug 25359
			if (ConfigurationManager.AppSettings.GetBoolSetting("EnableNewResourceCalculation"))
			{
				builder.RegisterType<ScheduledResourcesReadModelStorage>()
					.As<IScheduledResourcesReadModelPersister>()
					.As<IScheduledResourcesReadModelReader>()
					.SingleInstance();
				builder.RegisterType<ScheduledResourcesReadModelUpdater>()
					.As<IScheduledResourcesReadModelUpdater>().SingleInstance();
			}
			else
			{
				builder.RegisterType<DisabledScheduledResourcesReadModelStorage>()
					.As<IScheduledResourcesReadModelPersister>()
					.As<IScheduledResourcesReadModelReader>()
					.SingleInstance();
				builder.RegisterType<DisabledScheduledResourcesReadModelUpdater>()
					.As<IScheduledResourcesReadModelUpdater>().SingleInstance();
			}


		}
	}
}