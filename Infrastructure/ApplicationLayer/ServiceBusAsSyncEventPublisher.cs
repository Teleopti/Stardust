using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusAsSyncEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;

		public ServiceBusAsSyncEventPublisher(
			ResolveEventHandlers resolver, 
			CommonEventProcessor processor)
		{
			_resolver = resolver;
			_processor = processor;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
				foreach (var handlerType in _resolver.HandlerTypesFor<IRunOnServiceBus>(@event))
					_processor.Process(@event, handlerType);
		}
	}
}