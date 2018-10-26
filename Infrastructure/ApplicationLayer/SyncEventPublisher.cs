using System;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using AggregateException = System.AggregateException;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;
		private readonly ICurrentDataSource _dataSource;
		private readonly ISyncEventProcessingExceptionHandler _exceptionHandler;

		public SyncEventPublisher(ResolveEventHandlers resolver, CommonEventProcessor processor, ICurrentDataSource dataSource, ISyncEventProcessingExceptionHandler exceptionHandler)
		{
			_resolver = resolver;
			_processor = processor;
			_dataSource = dataSource;
			_exceptionHandler = exceptionHandler;
		}

		public void Publish(params IEvent[] events)
		{
			var jobs = _resolver.JobsFor<IRunInSync>(events);
			jobs.ForEach(execute);
		}

		private void execute(IJobInfo job)
		{
			var tenant = _dataSource.CurrentName();
			var exceptions = new List<Exception>();

			var attempts = job.Attempts;
			while (attempts-- > 0)
			{
				var thread = new Thread(() =>
				{
					try
					{
						_processor.Process(tenant, job.Event, job.Package, job.HandlerType);
						exceptions.Clear();
					}
					catch (Exception e)
					{
						exceptions.Add(e);
					}
				});
				thread.Start();
				thread.Join();

				if (exceptions.Count == 0)
					return;
			}

			if (exceptions.Count > 0)
				_exceptionHandler.Handle(new AggregateException(exceptions));
		}
	}
}