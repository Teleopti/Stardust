using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class LocalServiceBusEventsPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<LocalServiceBusPublisher>().As<IPublishEventsFromEventHandlers>().As<ISendDelayedMessages>().SingleInstance();
			builder.Register(c => c.Resolve<ISyncEventPublisher>()).As<IEventPublisher>().SingleInstance();
		}
	}
}