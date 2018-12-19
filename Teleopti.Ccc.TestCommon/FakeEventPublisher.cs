using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeEventPublisher : IEventPublisher
	{
		public IRtaEventPublisher RtaEventPublisher { get; set; }
		public ICurrentDataSource DataSource { get; set; }
		public ResolveEventHandlers EventHandlers { get; set; }
		public CommonEventProcessor EventProcessor { get; set; }

		private readonly List<Type> _handlerTypes = new List<Type>();
		private ConcurrentQueue<IEvent> _queuedEvents = new ConcurrentQueue<IEvent>();

		public IEvent[] PublishedEvents => _queuedEvents.ToArray();

		public void Clear() => _queuedEvents = new ConcurrentQueue<IEvent>();
		public void AddHandler<T>() => _handlerTypes.Add(typeof(T));
		public void AddHandler(Type type) => _handlerTypes.Add(type);

		public void Publish(params IEvent[] events)
		{
			RtaEventPublisher.Publish(events);

			events.ForEach(_queuedEvents.Enqueue);

			if (_handlerTypes.IsEmpty())
				return;

			var tenant = DataSource.CurrentName();

			var jobs = EventHandlers.ResolveAllJobs(events);

			// run in order of handlers added, sometimes the test is order dependent
			_handlerTypes.ForEach(handlerType =>
			{
				jobs.Where(x => x.HandlerType == handlerType)
					.ForEach(job =>
					{
						//
						onAnotherThread(() =>
						{
							//
							Console.WriteLine(job);
							EventProcessor.Process(tenant, job.Event, job.Package, job.HandlerType);
						});
					});
			});
		}

		private static void onAnotherThread(Action action)
		{
			var thread = new Thread(action.Invoke);
			thread.Start();
			thread.Join();
		}
	}
}