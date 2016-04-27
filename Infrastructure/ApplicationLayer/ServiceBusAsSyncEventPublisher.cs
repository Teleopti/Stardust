using System;
using System.Collections.Generic;
using System.Threading;
using NHibernate.Util;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
#pragma warning disable 618

	public class ServiceBusAsSyncEventPublisher : IEventPublisher
	{
		private readonly ServiceBusEventProcessor _processor;
		private readonly ResolveEventHandlers _resolver;
		private readonly Queue<IEvent> _queue = new Queue<IEvent>();
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
				startProcessingThread(() => processQueue(exceptions));
				if (exceptions.Any())
					throw new AggregateException(exceptions);
			}
			finally
			{
				Monitor.Exit(processLock);
			}
		}

		private static void startProcessingThread(ThreadStart run)
		{
			var thread = new Thread(run);
			thread.Start();
			thread.Join();
		}

		private void processQueue(ICollection<Exception> exceptions)
		{
			while (_queue.Any())
			{
				try
				{
					ProcessLikeTheBus(_queue.Dequeue());
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