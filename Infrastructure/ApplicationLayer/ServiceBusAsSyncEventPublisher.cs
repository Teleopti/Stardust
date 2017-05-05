using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using NHibernate.Util;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
#pragma warning disable 618

	public class ServiceBusAsSyncEventPublisher : IEventPublisher
	{
		private readonly ServiceBusEventProcessor _processor;
		private readonly ResolveEventHandlers _resolver;
		private readonly ConcurrentQueue<IEvent> _queue = new ConcurrentQueue<IEvent>();
		private readonly object processLock = new object();

		public ServiceBusAsSyncEventPublisher(
			ServiceBusEventProcessor processor, 
			ResolveEventHandlers resolver)
		{
			_processor = processor;
			_resolver = resolver;
		}

		public void Publish(params IEvent[] events)
		{
			events.ForEach(e =>
			{
				if (!_resolver.HandlerTypesFor<IRunOnServiceBus>(e).Any())
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
					IEvent @event;
					_queue.TryDequeue(out @event);
					if (@event != null)
						ProcessLikeTheBus(@event);
				}
				catch (Exception e)
				{
					PreserveStack.For(e);
					exceptions.Add(e);
				}
			}
		}

		[AsSystem]
		protected virtual void ProcessLikeTheBus(IEvent @event)
		{
			_processor.Process(@event);
		}

	}

#pragma warning restore 618
}