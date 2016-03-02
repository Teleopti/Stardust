using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class MultiEventPublisherServiceBusAsSync : IEventPublisher
	{
		private readonly HangfireEventPublisher _hangfirePublisher;
		private readonly ServiceBusAsSyncEventPublisher _serviceBusPublisher;
		private readonly StardustEventPublisher _stardustEventPublisher;
		private RunInProcessEventPublisher _runInProcessEventPublisher;

		public MultiEventPublisherServiceBusAsSync(
			HangfireEventPublisher hangfirePublisher, 
			ServiceBusAsSyncEventPublisher serviceBusPublisher,
			StardustEventPublisher stardustEventPublisher,
			RunInProcessEventPublisher runInProcessEventPublisher)
		{
			_runInProcessEventPublisher = runInProcessEventPublisher;
			_hangfirePublisher = hangfirePublisher;
			_serviceBusPublisher = serviceBusPublisher;
			_stardustEventPublisher = stardustEventPublisher;
		}

		public void Publish(params IEvent[] events)
		{
			_hangfirePublisher.Publish(events);
			_serviceBusPublisher.Publish(events);
			_stardustEventPublisher.Publish(events);
			_runInProcessEventPublisher.Publish(events);
		}
	}
}