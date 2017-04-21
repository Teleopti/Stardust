using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncAllEventPublisher : IEventPublisher
	{
		private readonly ServiceBusAsSyncEventPublisher _serviceBusAsSyncEventPublisher;
		private readonly HangfireAsSyncEventPublisher _hangfireAsSyncEventPublisher;
		private readonly SyncEventPublisher _syncEventPublisher;

		public SyncAllEventPublisher(
			ServiceBusAsSyncEventPublisher serviceBusAsSyncEventPublisher, 
			HangfireAsSyncEventPublisher hangfireAsSyncEventPublisher,
			SyncEventPublisher syncEventPublisher
			)
		{
			_serviceBusAsSyncEventPublisher = serviceBusAsSyncEventPublisher;
			_hangfireAsSyncEventPublisher = hangfireAsSyncEventPublisher;
			_syncEventPublisher = syncEventPublisher;
		}

		public void Publish(params IEvent[] events)
		{
			_hangfireAsSyncEventPublisher.Publish(events);
			_serviceBusAsSyncEventPublisher.Publish(events);
			_syncEventPublisher.Publish(events);
		}
	}
}