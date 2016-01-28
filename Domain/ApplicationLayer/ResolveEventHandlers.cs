using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
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

		private IEnumerable<object> resolveHandlersForEvent(IEvent @event)
		{
			var handlerType = typeof(IHandleEvent<>).MakeGenericType(@event.GetType());
			var enumerableHandlerType = typeof(IEnumerable<>).MakeGenericType(handlerType);
			return (_resolver.Resolve(enumerableHandlerType) as IEnumerable).Cast<object>();
		}

		public IEnumerable<object> ResolveHangfireHandlersForEvent(IEvent @event)
		{
			return resolveHandlersForEvent(@event)
				.OfType<IRunOnHangfire>();
		}

		public IEnumerable<object> ResolveServiceBusHandlersForEvent(IEvent @event)
		{
			return resolveHandlersForEvent(@event)
				.OfType<IRunOnServiceBus>();
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