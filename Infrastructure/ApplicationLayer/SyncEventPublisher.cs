using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Castle.Core.Internal;
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
		private readonly ISyncEventPublisherExceptionHandler _exceptionHandler;

		public SyncEventPublisher(ResolveEventHandlers resolver, CommonEventProcessor processor, ICurrentDataSource dataSource, ISyncEventPublisherExceptionHandler exceptionHandler)
		{
			_resolver = resolver;
			_processor = processor;
			_dataSource = dataSource;
			_exceptionHandler = exceptionHandler;
		}

		public void Publish(params IEvent[] events)
		{
			(
					from @event in events
					from handlerType in _resolver.HandlerTypesFor<IRunInSync>(@event)
					let attempts = _resolver.AttemptsFor(handlerType, @event)
					select new
					{
						handlerType,
						@event,
						attempts
					}
				)
				.ForEach(x =>
				{
					retry(x.handlerType, x.@event, x.attempts);
				});
		}

		private void retry(Type handlerType, IEvent @event, int attempts)
		{
			var tenant = _dataSource.CurrentName();
			var exceptions = new List<Exception>();

			while (attempts --> 0)
			{
				var thread = new Thread(() =>
				{
					try
					{
						_processor.Process(tenant, new []{ @event }, handlerType);
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