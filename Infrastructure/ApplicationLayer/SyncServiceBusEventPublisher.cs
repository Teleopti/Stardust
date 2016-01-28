using System.Linq;
using System.Reflection;
using Autofac;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncServiceBusEventPublisher : IEventPublisher, ICurrentEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;

		public SyncServiceBusEventPublisher(ResolveEventHandlers resolver)
		{
			_resolver = resolver;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				var handlers = _resolver.ResolveHandlersForEvent(@event)
					.Where(x => x.GetType().IsAssignableTo<IRunOnServiceBus>());

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

		public IEventPublisher Current()
		{
			return this;
		}
	}
}