using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class LocalServiceBusPublisher : IPublishEventsFromEventHandlers
	{
		private readonly IServiceBus _bus;

		public LocalServiceBusPublisher(IServiceBus bus)
		{
			_bus = bus;
		}

		public void Publish(object @event)
		{
			_bus.Send(@event);
		}
	}
}