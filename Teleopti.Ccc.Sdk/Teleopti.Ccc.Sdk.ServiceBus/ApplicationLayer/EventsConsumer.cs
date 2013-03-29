﻿using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.ApplicationLayer
{
	public class EventsConsumer : 
		ConsumerOf<IEvent>,
		ConsumerOf<EventsPackageMessage>
	{
		private readonly IEventPublisher _publisher;
		private readonly IServiceBus _bus;

		public EventsConsumer(IEventPublisher publisher, IServiceBus bus)
		{
			_publisher = publisher;
			_bus = bus;
		}

		public void Consume(IEvent message)
		{
			_publisher.Publish(message);
		}

		public void Consume(EventsPackageMessage message)
		{
			_bus.Send(message.Events);
		}
	}
}