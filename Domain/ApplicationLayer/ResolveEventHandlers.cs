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
			return HandlerTypesFor<T>(new[] {@event});
		}

		public IEnumerable<Type> HandlerTypesFor<T>(IEnumerable<IEvent> @events)
		{
			var multiHandlers = _resolve.ConcreteTypesFor(typeof(IHandleEvents));

			var normalTypes = events.Select(e => e.GetType())
					.Select(x => typeof(IHandleEvent<>).MakeGenericType(x))
					.SelectMany(x => _resolve.ConcreteTypesFor(x))
				;

			//var handlerType = typeof (IHandleEvent<>).MakeGenericType(@event.GetType());

			return normalTypes
				.Concat(multiHandlers)
				.Where(x => x.GetInterfaces().Contains(typeof (T)));
		}
		
		public MethodInfo HandleMethodFor(Type handler, IEvent @event)
		{
			if (handler.GetInterfaces().Contains(typeof(IHandleEvents)))
			{
				return handler
					.GetMethods()
					.FirstOrDefault(m =>
						m.Name == "Handle" &&
						m.GetParameters().Single().ParameterType == typeof(IEnumerable<IEvent>)
					);
			}

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

		public IEnumerable<IEvent> EventsFor(Type handlerType, IEvent[] events)
		{
			var subscriptions = new SubscriptionsRegistror();
			var method = handlerType
				.GetMethods()
				.FirstOrDefault(x => x.Name == "Subscribe");

			if (method == null)
				return events;

			method.Invoke(_resolve.Resolve(handlerType), new[] {subscriptions});

			return events.Where(x => subscriptions.Has(x.GetType())).ToArray();

		//	events.Where(e =>
		//		{
		//			return handlerType
		//				.GetMethods()
		//				.FirstOrDefault(x => x.Name == "Subscribe" && x.GetParameters().Single().ParameterType == e.GetType()
		//		})
		//		.Count() > 0
		//)

		//return handlerType
		//		.GetMethods()
		//		.FirstOrDefault(x => x.Name == "Subscribe" &&  
				   
		//		//events.Select(y=> y.GetType()).Contains(x.GetParameters().Single().ParameterType))
				
		//		?.Invoke(_resolve.Resolve(handlerType), events);
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
			return HandleMethodFor(handlerType, @event)?
				.GetCustomAttributes(typeof(AttemptsAttribute), true)
				.Cast<AttemptsAttribute>()
				.SingleOrDefault();
		}
	}
}