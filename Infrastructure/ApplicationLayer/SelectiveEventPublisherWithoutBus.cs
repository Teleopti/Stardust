using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SelectiveEventPublisherWithoutBus : IEventPublisher
	{
		private readonly HangfireEventPublisher _hangfirePublisher;
		private readonly SyncServiceBusEventPublisher _syncServiceBusEventPublisher;
		private readonly ResolveEventHandlers _resolver;

		public SelectiveEventPublisherWithoutBus(
			HangfireEventPublisher hangfirePublisher, 
			SyncServiceBusEventPublisher syncServiceBusEventPublisher, 
			ResolveEventHandlers resolver)
		{
			_hangfirePublisher = hangfirePublisher;
			_syncServiceBusEventPublisher = syncServiceBusEventPublisher;
			_resolver = resolver;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				var allHandlers = _resolver.ResolveHandlersForEvent(@event).ToArray();

				var hasHangfireHandlers = allHandlers.Any(x => x.GetType().IsAssignableTo<IRunOnHangfire>());
				var hasBusHandlers = allHandlers.Any(x => x.GetType().IsAssignableTo<IRunOnServiceBus>());

				if (hasHangfireHandlers)
					_hangfirePublisher.Publish(@event);

				if (hasBusHandlers)
					_syncServiceBusEventPublisher.Publish(@event);
			}
		}
	}
}