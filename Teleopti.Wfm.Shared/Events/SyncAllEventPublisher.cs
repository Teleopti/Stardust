using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncAllEventPublisher : IEventPublisher
	{
		private readonly HangfireAsSyncEventPublisher _hangfireAsSyncEventPublisher;
		private readonly SyncEventPublisher _syncEventPublisher;
		private readonly IRtaEventPublisher _rtaEventPublisher;

		public SyncAllEventPublisher(
			HangfireAsSyncEventPublisher hangfireAsSyncEventPublisher,
			SyncEventPublisher syncEventPublisher,
			IRtaEventPublisher rtaEventPublisher)
		{
			_hangfireAsSyncEventPublisher = hangfireAsSyncEventPublisher;
			_syncEventPublisher = syncEventPublisher;
			_rtaEventPublisher = rtaEventPublisher;
		}

		public void Publish(params IEvent[] events)
		{
			_hangfireAsSyncEventPublisher.Publish(events);
			_syncEventPublisher.Publish(events);
			_rtaEventPublisher.Publish(events);
		}
	}
}