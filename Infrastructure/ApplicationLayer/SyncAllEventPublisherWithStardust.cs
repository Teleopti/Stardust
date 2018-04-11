using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.RealTimeAdherence;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncAllEventPublisherWithStardust : IEventPublisher
	{
		private readonly HangfireAsSyncEventPublisher _hangfireAsSyncEventPublisher;
		private readonly SyncEventPublisher _syncEventPublisher;
		private readonly IRtaEventPublisher _rtaEventPublisher;
		private readonly StardustEventPublisher _stardustEventPublisher;

		public SyncAllEventPublisherWithStardust(
			HangfireAsSyncEventPublisher hangfireAsSyncEventPublisher,
			SyncEventPublisher syncEventPublisher,
			IRtaEventPublisher rtaEventPublisher,
			StardustEventPublisher stardustEventPublisher)
		{
			_hangfireAsSyncEventPublisher = hangfireAsSyncEventPublisher;
			_syncEventPublisher = syncEventPublisher;
			_rtaEventPublisher = rtaEventPublisher;
			_stardustEventPublisher = stardustEventPublisher;
		}

		public void Publish(params IEvent[] events)
		{
			_hangfireAsSyncEventPublisher.Publish(events);
			_syncEventPublisher.Publish(events);
			_rtaEventPublisher.Publish(events);
			_stardustEventPublisher.Publish(events);
		}
	}
}