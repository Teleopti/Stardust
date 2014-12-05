using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusEventPublisher : IServiceBusEventPublisher
	{
		private readonly IServiceBusSender _sender;

		public ServiceBusEventPublisher(IServiceBusSender sender)
		{
			_sender = sender;
		}

		public void Publish(IEvent @event)
		{
			_sender.Send(@event, true);
		}
	}
}