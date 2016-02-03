using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class MultiEventPublisherServiceBusAsSync : IEventPublisher
	{
		private readonly HangfireEventPublisher _hangfirePublisher;
		private readonly ServiceBusAsSyncEventPublisher _serviceBusPublisher;

		public MultiEventPublisherServiceBusAsSync(
			HangfireEventPublisher hangfirePublisher, 
			ServiceBusAsSyncEventPublisher serviceBusPublisher)
		{
			_hangfirePublisher = hangfirePublisher;
			_serviceBusPublisher = serviceBusPublisher;
		}

		public void Publish(params IEvent[] events)
		{
			_hangfirePublisher.Publish(events);
			_serviceBusPublisher.Publish(events);
		}
	}
}