using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SelectiveEventPublisherWithoutBus : IEventPublisher
	{
		private readonly IHangfireEventPublisher _hangfirePublisher;
		private readonly ISyncEventPublisher _syncEventPublisher;
		private readonly ResolveEventHandlers _resolver;

		public SelectiveEventPublisherWithoutBus(
			IHangfireEventPublisher hangfirePublisher, 
			ISyncEventPublisher syncEventPublisher, 
			ResolveEventHandlers resolver)
		{
			_hangfirePublisher = hangfirePublisher;
			_syncEventPublisher = syncEventPublisher;
			_resolver = resolver;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				var allHandlers = _resolver.ResolveHandlersForEvent(@event).ToList();

				var hasHangfireHandlers = allHandlers.Any(x => x.GetType().IsAssignableTo<IRunOnHangfire>());
				var hasBusHandlers = allHandlers.Any(x => !x.GetType().IsAssignableTo<IRunOnHangfire>());

				if (hasHangfireHandlers)
					_hangfirePublisher.Publish(@event);

				if (hasBusHandlers)
					_syncEventPublisher.Publish(@event);
			}
		}
	}
}