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

		public IEnumerable<JobInfo> JobsFor<T>(IEnumerable<IEvent> events)
		{
			var packages = 
				_resolve.ConcreteTypesFor(typeof(IHandleEvents))
				.Select(handler =>
				{
					var registrator = new subscriptionRegistrator();
					(_resolve.Resolve(handler) as dynamic).Subscribe(registrator);
					return new
					{
						events = events.Where(x => registrator.SubscribesTo(x.GetType())).ToArray(),
						handler
					};

				})
				.Where(x => x.events.Any())
				.Select(x => build(x.handler, typeof(IEnumerable<IEvent>), null, x.events))
				;

			var singles = from @event in events
				let type = typeof(IHandleEvent<>).MakeGenericType(@event.GetType())
				from handler in _resolve.ConcreteTypesFor(type)
				select build(handler, @event.GetType(), @event, null);

			return packages
				.Concat(singles)
				.Where(y => y.HandlerType.GetInterfaces().Contains(typeof(T)))
				.ToArray();
		}

		private JobInfo build(Type handler, Type handleMethodArgumentType, IEvent @event, IEnumerable<IEvent> package)
		{
			var handleMethod = HandleMethodFor(handler, handleMethodArgumentType);
			var allowFailuresAttribute = getAllowFailuresAttribute(handleMethod);

			return new JobInfo
			{
				HandlerType = handler,
				Event = @event,
				Queue = queueTo(handler, @event ?? package.First()),
				Package = package,
				Attempts = getAttemptsAttribute(handleMethod)?.Attempts ?? 3,
				AttemptsAttribute = getAttemptsAttribute(handleMethod),
				AllowFailures = allowFailuresAttribute?.Failures ?? 0,
				AllowFailuresAttribute = allowFailuresAttribute
			};
		}

		private string queueTo(Type handler, IEvent @event)
		{
			// check for interface? Naaahh...
			return handler
				.GetMethods()
				.FirstOrDefault(m =>
						m.Name == "QueueTo" &&
						m.GetParameters().Single().ParameterType == @event.GetType()
				)
				?.Invoke(_resolve.Resolve(handler), new[] { @event }) as string;
		}
		private AttemptsAttribute getAttemptsAttribute(MethodInfo handlerMethod)
		{
			return handlerMethod?
				.GetCustomAttributes(typeof(AttemptsAttribute), true)
				.Cast<AttemptsAttribute>()
				.SingleOrDefault();
		}

		private AllowFailuresAttribute getAllowFailuresAttribute(MethodInfo handlerMethod)
		{
			return handlerMethod?
				.GetCustomAttributes(typeof(AllowFailuresAttribute), true)
				.Cast<AllowFailuresAttribute>()
				.SingleOrDefault();
		}

		public MethodInfo HandleMethodFor(Type handler, Type argumentType)
		{
			return handler
				.GetMethods()
				.FirstOrDefault(m =>
					m.Name == "Handle" &&
					m.GetParameters().Single().ParameterType == argumentType
				);
		}

		public IEnumerable<Type> HandlerTypesFor<T>(IEvent @event)
		{
			var handlerType = typeof(IHandleEvent<>).MakeGenericType(@event.GetType());
			return _resolve.ConcreteTypesFor(handlerType)
				.Where(x => x.GetInterfaces().Contains(typeof(T)));
		}

		private class subscriptionRegistrator : ISubscriptionRegistrator
		{
			private readonly IList<Type> subscriptions = new List<Type>();

			public void SubscribeTo<T>() where T : IEvent
			{
				subscriptions.Add(typeof(T));
			}

			public bool SubscribesTo(Type type)
			{
				return subscriptions.Contains(type);
			}
		}
	}

	public class JobInfo
	{
		public Type HandlerType;
		public IEvent Event;
		public IEnumerable<IEvent> Package;

		public string Queue;

		public int Attempts;
		public AttemptsAttribute AttemptsAttribute;
		public int AllowFailures;
		public AllowFailuresAttribute AllowFailuresAttribute;
	}
}