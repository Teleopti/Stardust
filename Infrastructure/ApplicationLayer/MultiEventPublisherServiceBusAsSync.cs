using NHibernate.Properties;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.RealTimeAdherence;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class MultiEventPublisherServiceBusAsSync : IEventPublisher
	{
		private readonly HangfireEventPublisher _hangfirePublisher;
		private readonly StardustEventPublisher _stardustEventPublisher;
		private readonly SyncInFatClientProcessEventPublisher _syncInFatClientProcessEventPublisher;
		private readonly SyncEventPublisher _syncEventPublisher;
		private readonly IRtaEventPublisher _rtaEventPublisher;

		public MultiEventPublisherServiceBusAsSync(
			HangfireEventPublisher hangfirePublisher, 
			StardustEventPublisher stardustEventPublisher,
			SyncEventPublisher syncEventPublisher,
			SyncInFatClientProcessEventPublisher syncInFatClientProcessEventPublisher, 
			IRtaEventPublisher rtaEventPublisher)
		{
			_hangfirePublisher = hangfirePublisher;
			_stardustEventPublisher = stardustEventPublisher;
			_syncEventPublisher = syncEventPublisher;
			_syncInFatClientProcessEventPublisher = syncInFatClientProcessEventPublisher;
			_rtaEventPublisher = rtaEventPublisher;
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