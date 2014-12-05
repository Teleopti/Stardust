using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class LocalServiceBusEventsPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<EventPopulatingPublisher>().As<IEventPopulatingPublisher>().SingleInstance();
			builder.RegisterType<SyncEventPublisher>().As<IEventPublisher>().SingleInstance();
			builder.RegisterType<AutofacResolve>().As<IResolve>().SingleInstance();
			builder.RegisterType<LocalServiceBusPublisher>()
				.As<IPublishEventsFromEventHandlers>()
				.As<ISendDelayedMessages>()
				.SingleInstance();

			builder.RegisterType<NotifyTeleoptiRtaServiceToCheckForActivityChange>().As<INotifyRtaToCheckForActivityChange>().SingleInstance();
		}
	}
}