using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class LocalServiceBusEventsPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			// this has become a tad too complex...
			// LocalServiceBusPublisher puts events on the bus queue
			// LocalServiceBusEventPublisher publishes sync!
			builder.RegisterType<LocalServiceBusPublisher>().As<IPublishEventsFromEventHandlers>().As<ISendDelayedMessages>().SingleInstance();
			builder.Register(c => c.Resolve<LocalServiceBusEventPublisher>()).As<IEventPublisher>().SingleInstance();
		}
	}
}