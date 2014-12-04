using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class LocalInMemoryEventsPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<EventContextPopulator>().As<IEventContextPopulator>();
			builder.RegisterType<SyncEventsPublisher>().As<IEventsPublisher>().SingleInstance();
			builder.RegisterType<EventPopulatingPublisher>()
				.As<IEventPopulatingPublisher>()
				.As<IPublishEventsFromEventHandlers>()
				.SingleInstance();
			builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();
			builder.RegisterType<AutofacResolve>().As<IResolve>().SingleInstance();
			builder.RegisterType<IgnoreDelayedMessages>().As<ISendDelayedMessages>();
			builder.RegisterType<DontNotifyRtaToCheckForActivityChange>().As<INotifyRtaToCheckForActivityChange>();
		}
	}

	public class ServiceBusEventsPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<EventContextPopulator>().As<IEventContextPopulator>().SingleInstance();
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
			builder.RegisterType<ThrowOnNotifyRtaToCheckForActivityChange>().As<INotifyRtaToCheckForActivityChange>();
		}
	}
}