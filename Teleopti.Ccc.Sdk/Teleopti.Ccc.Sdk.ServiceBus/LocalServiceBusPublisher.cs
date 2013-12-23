using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class LocalServiceBusPublisher : 
		IPublishEventsFromEventHandlers,
		ISendDelayedMessages
	{
		private readonly IServiceBus _bus;
		private readonly IEventContextPopulator _eventContextPopulator;

		public LocalServiceBusPublisher(IServiceBus bus, IEventContextPopulator eventContextPopulator)
		{
			_bus = bus;
			_eventContextPopulator = eventContextPopulator;
		}

		public void Publish(IEvent @event)
		{
			_eventContextPopulator.PopulateEventContext(@event);
			_bus.Send(@event);
		}

		public void DelaySend(DateTime time, object message)
		{
			_bus.DelaySend(time, message);
		}
	}
}