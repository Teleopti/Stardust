using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncAllEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;
		private readonly ServiceBusAsSyncEventPublisher _serviceBusAsSyncEventPublisher;

		public SyncAllEventPublisher(
			ResolveEventHandlers resolver,
			CommonEventProcessor processor, 
			ServiceBusAsSyncEventPublisher serviceBusAsSyncEventPublisher)
		{
			_resolver = resolver;
			_processor = processor;
			_serviceBusAsSyncEventPublisher = serviceBusAsSyncEventPublisher;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				var handlerTypes = _resolver.HandlerTypesFor<IRunOnHangfire>(@event);
				foreach (var handlerType in handlerTypes)
				{
					_processor.Process(@event, handlerType);
				}
			}
			_serviceBusAsSyncEventPublisher.Publish(events);
		}
	}
}