using System;
using System.Collections.Generic;
using System.Threading;
using NHibernate.Util;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusAsSyncEventPublisher : IEventPublisher
	{
		private readonly ServiceBusEventProcessor _processor;
		private readonly Queue<IEvent> _queue = new Queue<IEvent>();
		private readonly object processLock = new object();

		public ServiceBusAsSyncEventPublisher(ServiceBusEventProcessor processor)
		{
			_processor = processor;
		}

#pragma warning disable 618

		public void Publish(params IEvent[] events)
		{
			events.ForEach(_queue.Enqueue);

			if (!Monitor.TryEnter(processLock, 0)) return;

			try
			{
				var exceptions = new List<Exception>();
				var thread = new Thread(() =>
				{
					try
					{
						while (_queue.Count > 0)
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
					catch (Exception e)
					{
						PreserveStack.For(e);
						exceptions.Add(e);
					}
				});
				thread.Start();
				thread.Join();
				if (exceptions.Any())
					throw new AggregateException(exceptions);
			}
			finally
			{
				Monitor.Exit(processLock);
			}

		}

		[AsSystem]
		protected virtual void ProcessLikeTheBus(IEvent @event)
		{
			_processor.Process(@event);
		}

#pragma warning restore 618

	}
}