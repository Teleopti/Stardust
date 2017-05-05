using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain.Collection;
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

		public IEnumerable<IJobInfo> MinutelyRecurringJobsFor<T>(IEvent @event)
		{
			var jobs = jobsFor<T>(new[] {@event});

			if (jobs.Any(x => x.AttemptsAttribute != null))
				throw new Exception("Retrying minutely recurring job is a bad idea");

			// different defaults, thats all
			jobs.ForEach(x =>
			{
				x.Attempts = 1;
				if (x.AllowFailuresAttribute == null)
					x.AllowFailures = 10;
			});

			return jobs;
		}

		public IEnumerable<IJobInfo> JobsFor<T>(IEnumerable<IEvent> events)
		{
			return jobsFor<T>(events);
		}

		private IEnumerable<jobInfo> jobsFor<T>(IEnumerable<IEvent> events)
		{
			var packages =
					_resolve.ConcreteTypesFor(typeof(IHandleEvents))
						.Select(handler =>
						{
							var registrator = new SubscriptionRegistrator();
							(_resolve.Resolve(handler) as dynamic).Subscribe(registrator);
							return new
							{
								events = events.Where(x => registrator.SubscribesTo(x.GetType())).ToArray(),
								handler
							};
						})
						.Where(x => x.events.Any())
						.Select(x => buildJobInfo(x.handler, typeof(IEnumerable<IEvent>), null, x.events))
				;

			var singles = from @event in events
				let type = typeof(IHandleEvent<>).MakeGenericType(@event.GetType())
				from handler in _resolve.ConcreteTypesFor(type)
				select buildJobInfo(handler, @event.GetType(), @event, null);

			return packages
				.Concat(singles)
				.Where(y => y.HandlerType.GetInterfaces().Contains(typeof(T)))
				.ToArray();
		}

		private jobInfo buildJobInfo(Type handler, Type handleMethodArgumentType, IEvent @event, IEnumerable<IEvent> package)
		{
			var handleMethod = HandleMethodFor(handler, handleMethodArgumentType);
			var attemptsAttribute = getAttemptsAttribute(handleMethod);
			var allowFailuresAttribute = getAllowFailuresAttribute(handleMethod);

			return new jobInfo
			{
				HandlerType = handler,
				Event = @event,
				QueueName = queueTo(handler, @event ?? package.First()),
				Package = package,
				Attempts = attemptsAttribute?.Attempts ?? 3,
				AttemptsAttribute = attemptsAttribute,
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

		private class jobInfo : IJobInfo
		{
			public Type HandlerType { get; set; }
			public IEvent Event { get; set; }
			public IEnumerable<IEvent> Package { get; set; }

			public string QueueName { get; set; }

			public int Attempts { get; set; }
			public int AllowFailures { get; set; }

			public AttemptsAttribute AttemptsAttribute { get; set; }
			public AllowFailuresAttribute AllowFailuresAttribute { get; set; }
		}
	}

	public interface IJobInfo
	{
		Type HandlerType { get; }
		IEvent Event { get; }
		IEnumerable<IEvent> Package { get; }

		string QueueName { get; }

		int Attempts { get; }
		int AllowFailures { get; }
	}

}