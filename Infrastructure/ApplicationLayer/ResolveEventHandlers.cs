using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ResolveEventHandlers : IResolveEventHandlers
	{
		private readonly IResolve _resolver;

		public ResolveEventHandlers(IResolve resolver)
		{
			_resolver = resolver;
		}

		public IEnumerable<object> ResolveHandlersForEvent(IEvent @event)
		{
			var handlerType = typeof(IHandleEvent<>).MakeGenericType(@event.GetType());
			var enumerableHandlerType = typeof(IEnumerable<>).MakeGenericType(handlerType);
			return (_resolver.Resolve(enumerableHandlerType) as IEnumerable).Cast<object>();
		}
	}
}