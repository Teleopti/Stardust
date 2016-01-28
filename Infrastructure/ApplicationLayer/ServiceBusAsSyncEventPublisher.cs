using System.Reflection;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusAsSyncEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;

		public ServiceBusAsSyncEventPublisher(ResolveEventHandlers resolver)
		{
			_resolver = resolver;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				var handlers = _resolver.ResolveServiceBusHandlersForEvent(@event);

				foreach (var handler in handlers)
				{
					var method = _resolver.HandleMethodFor(handler.GetType(), @event);
					try
					{
						method.Invoke(handler, new[] {@event});
					}
					catch (TargetInvocationException e)
					{
						PreserveStack.ForInnerOf(e);
						throw e;
					}
				}
			}
		}
	}
}