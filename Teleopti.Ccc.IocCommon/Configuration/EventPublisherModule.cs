using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class EventPublisherModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public EventPublisherModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<AutofacResolve>().As<IResolve>().SingleInstance();

			builder.RegisterType<SyncEventsPublisher>().As<IEventsPublisher>().SingleInstance();

			builder.RegisterType<EventContextPopulator>().As<IEventContextPopulator>().SingleInstance();
			builder.RegisterType<EventPopulatingPublisher>().As<IEventPopulatingPublisher>().SingleInstance();

			builder.RegisterType<SyncEventPublisher>().SingleInstance();
			builder.RegisterType<HangfireEventPublisher>()
				.AsSelf()
				.As<IRecurringEventPublisher>()
				.SingleInstance();
			builder.RegisterType<ServiceBusEventPublisher>().SingleInstance();
			builder.RegisterType<LocalServiceBusEventPublisher>().SingleInstance();

			builder.RegisterType<ResolveEventHandlers>().SingleInstance();

			if (_configuration.Toggle(Toggles.RTA_NewEventHangfireRTA_34333))
				builder.RegisterType<SelectiveEventPublisher>().As<IEventPublisher>().SingleInstance();
			else
				builder.Register(c => c.Resolve<ServiceBusEventPublisher>()).As<IEventPublisher>().SingleInstance();

			builder.RegisterType<CurrentEventPublisher>()
				.As<ICurrentEventPublisher>()
				.As<IEventPublisherScope>()
				.SingleInstance();

			builder.RegisterType<AllTenantRecurringEventPublisher>().SingleInstance();

			builder.RegisterType<CannotPublishToHangfire>().As<IHangfireEventClient>().SingleInstance();
			builder.RegisterType<CannotPublishEventsFromEventHandlers>().As<IPublishEventsFromEventHandlers>().SingleInstance();
			builder.RegisterType<CannotSendDelayedMessages>().As<ISendDelayedMessages>().SingleInstance();





			if (_configuration.Args().BehaviorTest)
			{
				if (_configuration.Toggle(Toggles.RTA_NewEventHangfireRTA_34333))
					builder.RegisterType<SelectiveEventPublisherWithoutBus>().As<IEventPublisher>().SingleInstance();
				else
					builder.Register(c => c.Resolve<SyncEventPublisher>()).As<IEventPublisher>().SingleInstance();
				builder.Register(c => c.Resolve<IEventPopulatingPublisher>() as EventPopulatingPublisher).As<IPublishEventsFromEventHandlers>().SingleInstance();
				builder.RegisterType<IgnoreDelayedMessages>().As<ISendDelayedMessages>().SingleInstance();
			}


		}
	}
	
}