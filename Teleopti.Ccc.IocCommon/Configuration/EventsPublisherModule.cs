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
			builder.RegisterType<EventPublisher>()
				.As<IEventPublisher>()
				.As<IPublishEventsFromEventHandlers>()
				.SingleInstance();
			builder.RegisterType<AutofacResolve>().As<IResolve>().SingleInstance();
			builder.RegisterType<IgnoreDelayedMessages>().As<ISendDelayedMessages>();
			builder.RegisterType<IgnoreGetUpdatedScheduleChangeFromTeleoptiRtaService>().As<IGetUpdatedScheduleChangeFromTeleoptiRtaService>();
		}
	}

	public class ServiceBusEventsPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<EventContextPopulator>().As<IEventContextPopulator>();
			builder.RegisterType<SyncEventsPublisher>().As<IEventsPublisher>().SingleInstance();
			builder.RegisterType<ServiceBusEventPublisher>()
				.As<IEventPublisher>()
				.As<IServiceBusEventPublisher>()
				.SingleInstance();
			builder.RegisterType<CannotPublishEventsFromEventHandlers>()
				.As<IPublishEventsFromEventHandlers>()
				.SingleInstance();
			builder.RegisterType<CannotSendDelayedMessages>().As<ISendDelayedMessages>();
			builder.RegisterType<CannotGetUpdatedScheduleChangeFromTeleoptiRtaService>().As<IGetUpdatedScheduleChangeFromTeleoptiRtaService>();
		}
	}
}