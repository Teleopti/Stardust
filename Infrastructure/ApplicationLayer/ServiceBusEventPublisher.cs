using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusEventPublisher : IEventPublisher
	{
		private readonly IServiceBusSender _sender;
		private readonly IEventContextPopulator _eventContextPopulator;

		public ServiceBusEventPublisher(IServiceBusSender sender, IEventContextPopulator eventContextPopulator)
		{
			_sender = sender;
			_eventContextPopulator = eventContextPopulator;
		}

		public void Publish(IEvent @event)
		{
			if (!_sender.EnsureBus())
				throw new ApplicationException("Cant find the bus, cant publish the event!");

			_eventContextPopulator.PopulateEventContext(@event);

			_sender.Send(@event);
		}
	}
}