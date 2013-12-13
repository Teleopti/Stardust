using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class LocalServiceBusPublisher : 
		IPublishEventsFromEventHandlers,
		ISendDelayedMessages
	{
		private readonly IServiceBus _bus;

		public LocalServiceBusPublisher(IServiceBus bus)
		{
			_bus = bus;
		}

		public void Publish(IEvent @event)
		{
			_bus.Send(@event);
		}

		public void DelaySend(DateTime time, object message)
		{
			_bus.DelaySend(time, message);
		}
	}
}