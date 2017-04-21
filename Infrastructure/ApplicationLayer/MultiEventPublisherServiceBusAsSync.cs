using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class MultiEventPublisherServiceBusAsSync : IEventPublisher
	{
		private readonly HangfireEventPublisher _hangfirePublisher;
		private readonly ServiceBusAsSyncEventPublisher _serviceBusPublisher;
		private readonly StardustEventPublisher _stardustEventPublisher;
		private readonly SyncInFatClientProcessEventPublisher _syncInFatClientProcessEventPublisher;
		private readonly SyncEventPublisher _syncEventPublisher;

		public MultiEventPublisherServiceBusAsSync(
			HangfireEventPublisher hangfirePublisher, 
			ServiceBusAsSyncEventPublisher serviceBusPublisher,
			StardustEventPublisher stardustEventPublisher,
			SyncEventPublisher syncEventPublisher,
			SyncInFatClientProcessEventPublisher syncInFatClientProcessEventPublisher
			)
		{
			_hangfirePublisher = hangfirePublisher;
			_serviceBusPublisher = serviceBusPublisher;
			_stardustEventPublisher = stardustEventPublisher;
			_syncEventPublisher = syncEventPublisher;
			_syncInFatClientProcessEventPublisher = syncInFatClientProcessEventPublisher;
		}

		public void Publish(params IEvent[] events)
		{
			_hangfirePublisher.Publish(events);
			_serviceBusPublisher.Publish(events);
			_stardustEventPublisher.Publish(events);
			_syncEventPublisher.Publish(events);
			_syncInFatClientProcessEventPublisher.Publish(events);
		}
	}
}