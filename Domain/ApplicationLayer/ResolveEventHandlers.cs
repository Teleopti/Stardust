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
			var handlerType = typeof(IHandleEvent<>).MakeGenericType(@event.GetType());
			return _resolve.ConcreteTypesFor(handlerType)
				.Where(x => x.GetInterfaces().Contains(typeof(T)));
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

		public IEnumerable<JobInfo> JobsFor<T>(IEnumerable<IEvent> events)
		{
			var packages = 
				_resolve.ConcreteTypesFor(typeof(IHandleEvents))
				.Select(handler =>
				{
					var subscriptions = new SubscriptionsRegistrator();
					handler
						.GetMethods()
						.FirstOrDefault(x => x.Name == "Subscribe")
						.Invoke(_resolve.Resolve(handler), new[] { subscriptions });
					return new
					{
						events = events.Where(x => subscriptions.Has(x.GetType())).ToArray(),
						handler
					};

				})
				.Where(x => x.events.Any())
				.Select(x => new JobInfo
				{
					HandlerType = x.handler,
					Package = x.events
				});

			var singles = from e in events
				let type = typeof(IHandleEvent<>).MakeGenericType(e.GetType())
				from t in _resolve.ConcreteTypesFor(type)
				select new JobInfo
				{
					HandlerType = t,
					Event = e
				};

			return packages
				.Concat(singles)
				.Where(y => y.HandlerType.GetInterfaces().Contains(typeof(T)))
				.ToArray();
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

	public class JobInfo
	{
		public Type HandlerType;
		public IEvent Event;
		public IEnumerable<IEvent> Package;
	}
}