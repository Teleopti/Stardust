using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class ResolveEventHandlers
	{
		private readonly IResolve _resolve;

		public ResolveEventHandlers(IResolve resolve)
		{
			_resolve = resolve;
		}

		public IEnumerable<Type> HandlerTypesFor<T>(IEvent @event)
		{
			var handlerType = typeof (IHandleEvent<>).MakeGenericType(@event.GetType());
			return _resolve.ConcreteTypesFor(handlerType)
				.Where(x => x.GetInterfaces().Contains(typeof (T)));
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

		public string QueueTo(Type handler, IEvent @event)
		{
			// check for interface? Naaahh...
			return handler
				.GetMethods()
				.FirstOrDefault(m =>
						m.Name == "QueueTo" &&
						m.GetParameters().Single().ParameterType == @event.GetType()
				)
				?.Invoke(_resolve.Resolve(handler), new[] {@event}) as string;
		}

		public int AttemptsFor(Type handlerType, IEvent @event)
		{
			return AttemptsFor(GetAttemptsAttribute(handlerType, @event));
		}

		public int AttemptsFor(AttemptsAttribute attemptsAttribute)
		{
			return attemptsAttribute?.Attempts ?? 3;
		}

		public AttemptsAttribute GetAttemptsAttribute(Type handlerType, IEvent @event)
		{
			return HandleMethodFor(handlerType, @event)
				.GetCustomAttributes(typeof(AttemptsAttribute), true)
				.Cast<AttemptsAttribute>()
				.SingleOrDefault();
		}
	}
}