using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class MultiEventPublisher : IEventPublisher
	{
		private readonly HangfireEventPublisher _hangfirePublisher;
		private readonly ServiceBusEventPublisher _serviceBusPublisher;
		private readonly StardustEventPublisher _stardustEventPublisher;
		private readonly RunInSyncInFatClientProcessEventPublisher _runInSyncInFatClientProcessEventPublisher;
		private readonly InSyncEventPublisher _inSyncEventPublisher;

		public MultiEventPublisher(
			HangfireEventPublisher hangfirePublisher, 
			ServiceBusEventPublisher serviceBusPublisher,
			StardustEventPublisher stardustEventPublisher,
			RunInSyncInFatClientProcessEventPublisher runInSyncInFatClientProcessEventPublisher, 
			InSyncEventPublisher inSyncEventPublisher)
		{
			_hangfirePublisher = hangfirePublisher;
			_serviceBusPublisher = serviceBusPublisher;
			_stardustEventPublisher = stardustEventPublisher;
			_runInSyncInFatClientProcessEventPublisher = runInSyncInFatClientProcessEventPublisher;
			_inSyncEventPublisher = inSyncEventPublisher;
		}

		public void Publish(params IEvent[] events)
		{
			_hangfirePublisher.Publish(events);
			_serviceBusPublisher.Publish(events);
			_stardustEventPublisher.Publish(events);
			_inSyncEventPublisher.Publish(events);
			_runInSyncInFatClientProcessEventPublisher.Publish(events);
		}
	}
}