using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection2;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class ResolveEventHandlers
	{
		private readonly IResolve _resolve;
		private readonly IConfigReader _config;

		public ResolveEventHandlers(IResolve resolve, IConfigReader config)
		{
			_resolve = resolve;
			_config = config;
		}

		[Obsolete("Use the method JobsFor<T> instead")]
		public IEnumerable<Type> HandlerTypesFor<T>(IEvent @event)
		{
			var handlerType = typeof(IHandleEvent<>).MakeGenericType(@event.GetType());
			return _resolve.ConcreteTypesFor(handlerType)
				.Where(x => x.GetInterfaces().Contains(typeof(T)));
		}

		public IEnumerable<IJobInfo> ResolveAllJobs(IEnumerable<IEvent> events) =>
			jobsFor<IRunOnHangfire>(events)
				.Concat(jobsFor<IRunOnStardust>(events))
				.Concat(jobsFor<IRunInSync>(events))
				.Concat(jobsFor<IRunInSyncInFatClientProcess>(events))
				.ToArray();

		public IEnumerable<IJobInfo> JobsFor<T>(IEnumerable<IEvent> events) =>
			jobsFor<T>(events);

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

		private IEnumerable<jobInfo> jobsFor<T>(IEnumerable<IEvent> events)
		{
			var packages =
					_resolve.ConcreteTypesFor(typeof(IHandleEvents))
						.SelectMany(handler =>
						{
							var registrator = new SubscriptionRegistrator();
							(_resolve.Resolve(handler) as dynamic).Subscribe(registrator);
							var subscribedEvents = events.Where(x => registrator.SubscribesTo(x.GetType())).ToArray();
							return subscribedEvents
								// 250 default is just a number, 
								// we saw no upper limit, 
								// but no performance difference between 1000 and 250 either.
								.Batch(_config.ReadValue("EventMaxPackageSize", 250))
								.Select(x => new
								{
									events = subscribedEvents,
									handler
								});
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
			var handleMethod = handler
				.GetMethods()
				.FirstOrDefault(m =>
					m.Name == "Handle" &&
					m.GetParameters().Single().ParameterType == handleMethodArgumentType
				);
			var attemptsAttribute = getAttemptsAttribute(handleMethod);
			var allowFailuresAttribute = getAllowFailuresAttribute(handleMethod);

			var runInterval = getRunIntervalForHandler(handler);

			return new jobInfo
			{
				HandlerType = handler,
				Event = @event,
				QueueName = queueTo(handler, @event ?? package.First()),
				Package = package,
				Attempts = attemptsAttribute?.Attempts ?? 3,
				AttemptsAttribute = attemptsAttribute,
				AllowFailures = allowFailuresAttribute?.Failures ?? 0,
				AllowFailuresAttribute = allowFailuresAttribute,
				RunInterval = runInterval
			};
		}

		private int getRunIntervalForHandler(Type handler)
		{
			var runIntervalAttribute = handler.GetCustomAttribute<RunIntervalAttribute>(false);
			if (runIntervalAttribute == null) return 1;
			var runIntervalValue = runIntervalAttribute.RunInterval;
			if (runIntervalValue < 1) return 1;
			return runIntervalValue;
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
				?.Invoke(_resolve.Resolve(handler), new[] {@event}) as string;
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

		private class jobInfo : IJobInfo
		{
			public Type HandlerType { get; set; }
			public IEvent Event { get; set; }
			public IEnumerable<IEvent> Package { get; set; }

			public string QueueName { get; set; }

			public int Attempts { get; set; }
			public int AllowFailures { get; set; }
			public int RunInterval { get; set; }

			public AttemptsAttribute AttemptsAttribute { get; set; }
			public AllowFailuresAttribute AllowFailuresAttribute { get; set; }

			public override string ToString() => $"{HandlerType?.Name} got {Event?.GetType().Name}";
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
		int RunInterval { get; }
	}
}