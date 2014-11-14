using System;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusEventPopulatingPublisher : IServiceBusEventPopulatingPublisher
	{
		private readonly IServiceBusEventPublisher _publisher;
		private readonly IEventContextPopulator _eventContextPopulator;

		public ServiceBusEventPopulatingPublisher(IServiceBusEventPublisher publisher, IEventContextPopulator eventContextPopulator)
		{
			_publisher = publisher;
			_eventContextPopulator = eventContextPopulator;
		}

		public void Publish(IEvent @event)
		{
			if (!_publisher.EnsureBus())
				throw new ApplicationException("Cant find the bus, cant publish the event!");
			_eventContextPopulator.PopulateEventContext(@event);
			_publisher.Publish(@event);
		}

		public void Publish(ILogOnInfo nonEvent)
		{
			if (!_publisher.EnsureBus())
				throw new ApplicationException("Cant find the bus, cant publish the ... non-event!");
			_eventContextPopulator.PopulateEventContext(nonEvent);
			_publisher.Publish(nonEvent);
		}

		public bool EnsureBus()
		{
			return _publisher.EnsureBus();
		}

	}
}