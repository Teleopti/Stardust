using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusEventPublisher : IEventPublisher
	{
		private readonly IServiceBusSender _sender;

		public ServiceBusEventPublisher(IServiceBusSender sender)
		{
			_sender = sender;
		}

		public void Publish(IEvent @event)
		{
			if (!_sender.EnsureBus())
				throw new Exception("Cant find the bus, cant publish the event!");
			_sender.Send(@event);
		}
	}
}