using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireOrBusEventPublisher : IEventPublisher
	{
		private readonly IHangfireEventPublisher _hangfirePublisher;
		private readonly IServiceBusEventPublisher _serviceBusPublisher;

		public HangfireOrBusEventPublisher(IHangfireEventPublisher hangfirePublisher, IServiceBusEventPublisher serviceBusPublisher)
		{
			_hangfirePublisher = hangfirePublisher;
			_serviceBusPublisher = serviceBusPublisher;
		}

		public void Publish(IEvent @event)
		{
			if (@event is IGoToHangfire)
				_hangfirePublisher.Publish(@event);
			else
				_serviceBusPublisher.Publish(@event);
		}
	}
}