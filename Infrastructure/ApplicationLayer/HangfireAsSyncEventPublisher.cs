using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NHibernate.Util;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireAsSyncEventPublisher : IEventPublisher
	{
		private readonly HangfireEventProcessor _processor;
		private readonly ResolveEventHandlers _resolver;
		private readonly Queue<IEvent> _queue = new Queue<IEvent>();
		private readonly object processLock = new object();

		public HangfireAsSyncEventPublisher(
			HangfireEventProcessor processor,
			ResolveEventHandlers resolver)
		{
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

			if (_queue.Any() && Monitor.TryEnter(processLock, 0))
				process();
		}

		private void process()
		{
			try
			{
				var exceptions = new List<Exception>();
				onAnotherThread(() => processQueue(exceptions));
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

		private void processQueue(ICollection<Exception> exceptions)
		{
			while (_queue.Any())
			{
				try
				{
					_processor.Process(_queue.Dequeue());
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