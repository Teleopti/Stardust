using System.Reflection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class SyncPublishTo : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly object[] _handlers;

		public SyncPublishTo(ResolveEventHandlers resolver, object handler)
		{
			_resolver = resolver;
			_handlers = new[] { handler };
		}

		public SyncPublishTo(ResolveEventHandlers resolver, object[] handlers)
		{
			_resolver = resolver;
			_handlers = handlers;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				foreach (var handler in _handlers)
				{
					var method = _resolver.HandleMethodFor(handler.GetType(), @event);
					if (method == null)
						continue;
					try
					{

						method.Invoke(handler, new[] { @event });
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