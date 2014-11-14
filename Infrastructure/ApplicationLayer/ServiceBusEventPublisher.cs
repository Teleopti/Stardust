using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusEventPublisher : IServiceBusEventPublisher
	{
		private readonly IServiceBusSender _sender;

		public ServiceBusEventPublisher(IServiceBusSender sender)
		{
			_sender = sender;
		}

		public bool EnsureBus()
		{
			return _sender.EnsureBus();
		}

		public void Publish(IEvent @event)
		{
			_sender.Send(@event);
		}

		public void Publish(ILogOnInfo nonEvent)
		{
			_sender.Send(nonEvent);
		}
	}
}