using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class EventPublisher : IEventPublisher
	{
		private readonly IResolve _resolver;

		public EventPublisher(IResolve resolver)
		{
			_resolver = resolver;
		}

		public void Publish(IEvent @event)
		{
			var handlerType = typeof(IHandleEvent<>).MakeGenericType(@event.GetType());
			var enumerableHandlerType = typeof(IEnumerable<>).MakeGenericType(handlerType);
			var handlers = _resolver.Resolve(enumerableHandlerType) as IEnumerable;
			if (handlers == null) return;

			foreach (var handler in handlers)
			{
				var method = handler.GetType().GetMethods()
					.Single(m => m.Name == "Handle" && m.GetParameters().Single().ParameterType == @event.GetType());
				method.Invoke(handler, new[] { @event });
			}
		}
	}
}