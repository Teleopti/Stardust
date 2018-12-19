using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection2;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Util2;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireAsSyncEventPublisher : IEventPublisher
	{
		private readonly ICurrentDataSource _dataSource;
		private readonly HangfireEventProcessor _processor;
		private readonly ResolveEventHandlers _resolver;
		private readonly ConcurrentQueue<IJobInfo> _queue = new ConcurrentQueue<IJobInfo>();
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
			_resolver.JobsFor<IRunOnHangfire>(events).ForEach(e =>
			{
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
				Retry.Handle<Exception>()
					.WaitAndRetry(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5))
					.Do(() =>
					{
						var exceptions = new List<Exception>();
						onAnotherThread(() => processQueue(tenant, exceptions));
						if (exceptions.Any())
							throw new AggregateException(exceptions);
					});
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
			IJobInfo job;
			while (_queue.TryDequeue(out job))
			{
				try
				{
					_processor.Process(null, tenant, job.Event, job.Package, job.HandlerType);
				}
				catch (Exception e)
				{
					exceptions.Add(e);
				}
			}
		}

	}
}