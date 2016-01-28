using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class MultiEventPublisherWithoutBus : IEventPublisher
	{
		private readonly HangfireEventPublisher _hangfirePublisher;
		private readonly ServiceBusAsSyncEventPublisher _serviceBusPublisher;
		private readonly ResolveEventHandlers _resolver;

		public MultiEventPublisherWithoutBus(
			HangfireEventPublisher hangfirePublisher, 
			ServiceBusAsSyncEventPublisher serviceBusPublisher, 
			ResolveEventHandlers resolver)
		{
			_hangfirePublisher = hangfirePublisher;
			_serviceBusPublisher = serviceBusPublisher;
			_resolver = resolver;
		}

		public void Publish(params IEvent[] events)
		{
			_hangfirePublisher.Publish(events);
			_serviceBusPublisher.Publish(events);
		}
	}
}