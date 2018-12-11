using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeEventPublisher : IEventPublisher
	{
		private readonly IRtaEventPublisher _rtaPublisher;
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;
		private readonly ICurrentDataSource _dataSource;
		private readonly List<Type> _handlerTypes = new List<Type>();
		private ConcurrentQueue<IEvent> queuedEvents = new ConcurrentQueue<IEvent>();

		public FakeEventPublisher(
			IRtaEventPublisher rtaPublisher,
			ResolveEventHandlers resolver,
			CommonEventProcessor processor,
			ICurrentDataSource dataSource)
		{
			_rtaPublisher = rtaPublisher;
			_resolver = resolver;
			_processor = processor;
			_dataSource = dataSource;
		}

		public IEvent[] PublishedEvents => queuedEvents.ToArray();

		public void Clear()
		{
			queuedEvents = new ConcurrentQueue<IEvent>();
		}

		public void AddHandler<T>()
		{
			_handlerTypes.Add(typeof(T));
		}

		public void AddHandler(Type type)
		{
			_handlerTypes.Add(type);
		}

		public void Publish(params IEvent[] events)
		{
			_rtaPublisher.Publish(events);

			events.ForEach(queuedEvents.Enqueue);

			if (_handlerTypes.IsEmpty())
				return;

			var tenant = _dataSource.CurrentName();

			var jobs = _resolver.ResolveAllJobs(events);
			
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
							_processor.Process(tenant, job.Event, job.Package, job.HandlerType);
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