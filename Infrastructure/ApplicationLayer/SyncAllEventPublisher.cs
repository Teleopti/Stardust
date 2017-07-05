using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncAllEventPublisher : IEventPublisher
	{
		private readonly HangfireAsSyncEventPublisher _hangfireAsSyncEventPublisher;
		private readonly SyncEventPublisher _syncEventPublisher;

		public SyncAllEventPublisher(
			HangfireAsSyncEventPublisher hangfireAsSyncEventPublisher,
			SyncEventPublisher syncEventPublisher
			)
		{
			_hangfireAsSyncEventPublisher = hangfireAsSyncEventPublisher;
			_syncEventPublisher = syncEventPublisher;
		}

		public void Publish(params IEvent[] events)
		{
			_hangfireAsSyncEventPublisher.Publish(events);
			_syncEventPublisher.Publish(events);
		}
	}
}