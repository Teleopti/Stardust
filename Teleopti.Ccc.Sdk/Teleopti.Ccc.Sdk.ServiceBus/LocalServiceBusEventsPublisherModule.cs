using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class LocalServiceBusEventsPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<LocalServiceBusEventPublisher>()
				.As<IEventPublisher>()
				.As<IDelayedMessageSender>()
				.SingleInstance();
		}
	}
}