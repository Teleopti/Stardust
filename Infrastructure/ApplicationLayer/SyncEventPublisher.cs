using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncEventPublisher : ISyncEventPublisher, ICurrentEventPublisher
	{
		private readonly IResolveEventHandlers _resolver;

		public SyncEventPublisher(IResolveEventHandlers resolver)
		{
			_resolver = resolver;
		}

		public void Publish(IEvent @event)
		{
			var handlers = _resolver.ResolveHandlersForEvent(@event);
			if (handlers == null) return;

			foreach (var handler in handlers)
			{
				var method = handler.GetType().GetMethods()
					.Single(m => m.Name == "Handle" && m.GetParameters().Single().ParameterType == @event.GetType());
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

		public IEventPublisher Current()
		{
			return this;
		}
	}
}