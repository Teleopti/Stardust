using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class MultiEventPublisher : IEventPublisher
	{
		private readonly HangfireEventPublisher _hangfirePublisher;
		private readonly StardustEventPublisher _stardustEventPublisher;
		private readonly SyncInFatClientProcessEventPublisher _syncInFatClientProcessEventPublisher;
		private readonly SyncEventPublisher _syncEventPublisher;
		private RtaEventPublisher _rtaEventPublisher;

		public MultiEventPublisher(
			HangfireEventPublisher hangfirePublisher,
			StardustEventPublisher stardustEventPublisher,
			SyncEventPublisher syncEventPublisher,
			SyncInFatClientProcessEventPublisher syncInFatClientProcessEventPublisher,
			RtaEventPublisher rtaEventPublisher
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