using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NHibernate.Util;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using AggregateException = System.AggregateException;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireAsSyncEventPublisher : IEventPublisher
	{
		private readonly ICurrentDataSource _dataSource;
		private readonly HangfireEventProcessor _processor;
		private readonly ResolveEventHandlers _resolver;
		private readonly ConcurrentQueue<IEvent> _queue = new ConcurrentQueue<IEvent>();
		private readonly object processLock = new object();

		public HangfireAsSyncEventPublisher(
			ICurrentDataSource dataSource,
			HangfireEventProcessor processor,
			ResolveEventHandlers resolver)
		{
			_dataSource = dataSource;
			_processor = processor;
			_resolver = resolver;
		}

		public void Publish(params IEvent[] events)
		{
			events.ForEach(e =>
			{
				if (!_resolver.HandlerTypesFor<IRunOnHangfire>(e).Any())
					return;
				_queue.Enqueue(e);
			});

			if (_queue.Count > 0 && Monitor.TryEnter(processLock, 0))
				process();
		}

		private void process()
		{
			var tenant = _dataSource.CurrentName();
			try
			{
				var exceptions = new List<Exception>();
				onAnotherThread(() => processQueue(tenant, exceptions));
				if (exceptions.Any())
					throw new AggregateException(exceptions);
			}
			finally
			{
				Monitor.Exit(processLock);
			}
		}

		private static void onAnotherThread(Action action)
		{
			var thread = new Thread(action.Invoke);
			thread.Start();
			thread.Join();
		}

		private void processQueue(string tenant, ICollection<Exception> exceptions)
		{
			IEvent @event;
			while (_queue.TryDequeue(out @event))
			{
				foreach (var handler in _resolver.HandlerTypesFor<IRunOnHangfire>(@event))
				{
					try
					{
						_processor.Process(tenant, @event, handler);
					}
					catch (Exception e)
					{
						PreserveStack.For(e);
						exceptions.Add(e);
					}
				}
			}
		}

	}
}