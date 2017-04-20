using System;
using System.Linq;
using System.Threading;
using Castle.Core.Internal;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class InSyncEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;
		private readonly IExceptionRethrower _rethrower;

		public InSyncEventPublisher(ResolveEventHandlers resolver, CommonEventProcessor processor, IExceptionRethrower rethrower)
		{
			_resolver = resolver;
			_processor = processor;
			_rethrower = rethrower;
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
			Exception ex = null;
			var thread = new Thread(() =>
			{
				while (attempts --> 0)
				{
					try
					{
						_processor.Process(@event, handlerType);
						ex = null;
						return;
					}
					catch (Exception e)
					{
						ex = e;
					}
				}
			});
			thread.Start();
			thread.Join();

			if (ex != null)
				_rethrower.Rethrow(ex);
		}
	}
}