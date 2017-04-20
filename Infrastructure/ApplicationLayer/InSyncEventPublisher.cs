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

		public InSyncEventPublisher(ResolveEventHandlers resolver, CommonEventProcessor processor)
		{
			_resolver = resolver;
			_processor = processor;
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
					onAnotherThread(() => retry(x.handlerType, x.@event, x.attempts));
				});
		}

		private void retry(Type handlerType, IEvent @event, int attempts)
		{
			while (attempts --> 0)
			{
				try
				{
					_processor.Process(@event, handlerType);
					return;
				}
				catch
				{
					// ignored
				}
			}
		}

		private static void onAnotherThread(Action action)
		{
			var thread = new Thread(action.Invoke);
			thread.Start();
			thread.Join();
		}
	}
}