using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class LocalServiceBusEventPublisher : 
		IEventPublisher,
		IDelayedMessageSender
	{
		private readonly IServiceBus _bus;
		private readonly IEventContextPopulator _eventContextPopulator;

		public LocalServiceBusEventPublisher(IServiceBus bus, IEventContextPopulator eventContextPopulator)
		{
			_bus = bus;
			_eventContextPopulator = eventContextPopulator;
		}

		public void Publish(params IEvent[] events)
		{
			_eventContextPopulator.PopulateEventContext(events);
			_bus.Send(events);
		}

		public void DelaySend(DateTime time, object message)
		{
			_bus.DelaySend(time, message);
		}
	}
}