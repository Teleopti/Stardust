using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class LocalInMemoryEventsPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SyncEventsPublisher>().As<IEventsPublisher>().SingleInstance();
			builder.RegisterType<EventPopulatingPublisher>()
				.As<IEventPopulatingPublisher>()
				.As<IPublishEventsFromEventHandlers>()
				.SingleInstance();
			builder.RegisterType<SyncEventPublisher>().As<IEventPublisher>().SingleInstance();
			builder.RegisterType<IgnoreDelayedMessages>().As<ISendDelayedMessages>();
		}
	}

	public class ServiceBusEventsPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SyncEventsPublisher>().As<IEventsPublisher>().SingleInstance();
			builder.RegisterType<EventPopulatingPublisher>()
				.As<IEventPopulatingPublisher>()
				.As<IPublishEventsFromEventHandlers>()
				.SingleInstance();
			builder.RegisterType<ServiceBusEventPublisher>().As<IEventPublisher>().SingleInstance();
			builder.RegisterType<CannotPublishEventsFromEventHandlers>()
				.As<IPublishEventsFromEventHandlers>()
				.SingleInstance();
			builder.RegisterType<CannotSendDelayedMessages>().As<ISendDelayedMessages>();
		}
	}
}