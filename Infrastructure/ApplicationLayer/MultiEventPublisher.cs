using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class MultiEventPublisher : IEventPublisher
	{
		private readonly HangfireEventPublisher _hangfirePublisher;
		private readonly ServiceBusEventPublisher _serviceBusPublisher;
		private readonly StardustEventPublisher _stardustEventPublisher;
		private readonly RunInProcessEventPublisher _runInProcessEventPublisher;

		public MultiEventPublisher(
			HangfireEventPublisher hangfirePublisher, 
			ServiceBusEventPublisher serviceBusPublisher,
			StardustEventPublisher stardustEventPublisher,
			RunInProcessEventPublisher runInProcessEventPublisher)
		{
			_hangfirePublisher = hangfirePublisher;
			_serviceBusPublisher = serviceBusPublisher;
			_stardustEventPublisher = stardustEventPublisher;
			_runInProcessEventPublisher = runInProcessEventPublisher;
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