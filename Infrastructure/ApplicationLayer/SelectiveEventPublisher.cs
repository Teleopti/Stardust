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
		private readonly ResolveEventHandlers _resolver;

		public SelectiveEventPublisher(
			IHangfireEventPublisher hangfirePublisher, 
			IServiceBusEventPublisher serviceBusPublisher,
			ResolveEventHandlers resolver)
		{
			_hangfirePublisher = hangfirePublisher;
			_serviceBusPublisher = serviceBusPublisher;
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
					_serviceBusPublisher.Publish(@event);
			}
		}
	}
}