using System;
using System.Linq;
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
		private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;

		public LocalServiceBusEventPublisher(IServiceBus bus, IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator)
		{
			_bus = bus;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
		}

		public void Publish(params IEvent[] events)
		{
			if (events.Any())
			{
				_eventInfrastructureInfoPopulator.PopulateEventContext (events);
				_bus.Send (events);
			}
		}

		public void DelaySend(DateTime time, object message)
		{
			_bus.DelaySend(time, message);
		}
	}
}