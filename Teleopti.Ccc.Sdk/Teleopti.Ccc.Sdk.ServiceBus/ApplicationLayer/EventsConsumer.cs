using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.ApplicationLayer
{
	public class EventsConsumer : ConsumerOf<IEvent>
	{
		private readonly IEventPublisher _publisher;

		public EventsConsumer(IEventPublisher publisher) {
			_publisher = publisher;
		}

		public void Consume(IEvent message)
		{
			_publisher.Publish(message);
		}
	}
}