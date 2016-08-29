using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncAllEventPublisher : IEventPublisher
	{
		private readonly ServiceBusAsSyncEventPublisher _serviceBusAsSyncEventPublisher;
		private readonly HangfireAsSyncEventPublisher _hangfireAsSyncEventPublisher;

		public SyncAllEventPublisher(
			ServiceBusAsSyncEventPublisher serviceBusAsSyncEventPublisher, 
			HangfireAsSyncEventPublisher hangfireAsSyncEventPublisher
			)
		{
			_serviceBusAsSyncEventPublisher = serviceBusAsSyncEventPublisher;
			_hangfireAsSyncEventPublisher = hangfireAsSyncEventPublisher;
		}

		public void Publish(params IEvent[] events)
		{
			_hangfireAsSyncEventPublisher.Publish(events);
			_serviceBusAsSyncEventPublisher.Publish(events);
		}
	}
}