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

		private IEnumerable<object> resolveHandlersForEvent(IEvent @event, IResolve resolver)
		{
			var handlerType = typeof(IHandleEvent<>).MakeGenericType(@event.GetType());
			var enumerableHandlerType = typeof(IEnumerable<>).MakeGenericType(handlerType);
			return (resolver.Resolve(enumerableHandlerType) as IEnumerable).Cast<object>();
		}

		public IEnumerable<object> ResolveHangfireHandlersForEvent(IEvent @event)
		{
			return resolveHandlersForEvent(@event, _resolver)
				.OfType<IRunOnHangfire>();
		}

		public IEnumerable<object> ResolveServiceBusHandlersForEvent(IEvent @event)
		{
			return resolveHandlersForEvent(@event, _resolver)
				.OfType<IRunOnServiceBus>();
		}

		public IEnumerable<object> ResolveStardustHandlersForEvent(IEvent @event)
		{
			return resolveHandlersForEvent(@event, _resolver)
				.OfType<IRunOnStardust>();
		}
		
		public IEnumerable<object> ResolveInProcessForEvent(IEvent @event, IResolve scope)
		{
				return resolveHandlersForEvent(@event, scope)
					.OfType<IRunInProcess>();
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