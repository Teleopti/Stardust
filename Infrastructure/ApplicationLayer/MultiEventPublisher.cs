using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.RealTimeAdherence;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class MultiEventPublisher : IEventPublisher
	{
		private readonly HangfireEventPublisher _hangfirePublisher;
		private readonly StardustEventPublisher _stardustEventPublisher;
		private readonly SyncInFatClientProcessEventPublisher _syncInFatClientProcessEventPublisher;
		private readonly SyncEventPublisher _syncEventPublisher;
		private readonly IRtaEventPublisher _rtaEventPublisher;

		public MultiEventPublisher(
			HangfireEventPublisher hangfirePublisher,
			StardustEventPublisher stardustEventPublisher,
			SyncEventPublisher syncEventPublisher,
			SyncInFatClientProcessEventPublisher syncInFatClientProcessEventPublisher,
			IRtaEventPublisher rtaEventPublisher
		)
		{
			_rtaEventPublisher = rtaEventPublisher;
			_hangfirePublisher = hangfirePublisher;
			_stardustEventPublisher = stardustEventPublisher;
			_syncEventPublisher = syncEventPublisher;
			_syncInFatClientProcessEventPublisher = syncInFatClientProcessEventPublisher;
		}

		public void Publish(params IEvent[] events)
		{
			_hangfirePublisher.Publish(events);
			_stardustEventPublisher.Publish(events);
			_syncEventPublisher.Publish(events);
			_syncInFatClientProcessEventPublisher.Publish(events);
			_rtaEventPublisher.Publish(events);
		}
	}
}