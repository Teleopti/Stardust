using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SelectiveEventPublisher : IEventPublisher
	{
		private readonly IHangfireEventPublisher _hangfirePublisher;
		private readonly IServiceBusEventPublisher _serviceBusPublisher;
		private readonly IResolveEventHandlers _resolveEventHandlers;

		public SelectiveEventPublisher(
			IHangfireEventPublisher hangfirePublisher, 
			IServiceBusEventPublisher serviceBusPublisher,
			IResolveEventHandlers resolveEventHandlers)
		{
			_hangfirePublisher = hangfirePublisher;
			_serviceBusPublisher = serviceBusPublisher;
			_resolveEventHandlers = resolveEventHandlers;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				var allHandlers = _resolveEventHandlers.ResolveHandlersForEvent(@event).Cast<object>().ToList();

				var hasHangfireHandlers = allHandlers.Any(x => x.GetType().IsAssignableTo<IRunOnHangfire>());
				var hasBusHandlers = allHandlers.Any(x => !x.GetType().IsAssignableTo<IRunOnHangfire>());

				if (hasHangfireHandlers)
					_hangfirePublisher.Publish(@event);

				if (hasBusHandlers)
					_serviceBusPublisher.Publish(@event);
			}
		}
	}

	public class SelectiveEventPublisherWithoutBus : IEventPublisher
	{
		private readonly IHangfireEventPublisher _hangfirePublisher;
		private readonly ISyncEventPublisher _syncEventPublisher;
		private readonly IResolveEventHandlers _resolveEventHandlers;

		public SelectiveEventPublisherWithoutBus(
			IHangfireEventPublisher hangfirePublisher, 
			ISyncEventPublisher syncEventPublisher, 
			IResolveEventHandlers resolveEventHandlers)
		{
			_hangfirePublisher = hangfirePublisher;
			_syncEventPublisher = syncEventPublisher;
			_resolveEventHandlers = resolveEventHandlers;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				var allHandlers = _resolveEventHandlers.ResolveHandlersForEvent(@event).Cast<object>().ToList();

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