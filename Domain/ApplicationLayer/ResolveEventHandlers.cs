using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class ResolveEventHandlers
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

		public MethodInfo HandleMethodFor(Type handler, IEvent @event)
		{
			return handler
				.GetMethods()
				.FirstOrDefault(m =>
					m.Name == "Handle" &&
					m.GetParameters().Single().ParameterType == @event.GetType()
				);
		}

	}
}