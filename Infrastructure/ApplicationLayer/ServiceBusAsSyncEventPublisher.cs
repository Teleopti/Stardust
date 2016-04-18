using System;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusAsSyncEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly ServiceBusEventProcessor _processor;

		public ServiceBusAsSyncEventPublisher(ResolveEventHandlers resolver, ServiceBusEventProcessor processor)
		{
			_resolver = resolver;
			_processor = processor;
		}

		public void Publish(params IEvent[] events)
		{
			events.Where(e => _resolver.HandlerTypesFor<IRunOnServiceBus>(e).Any())
				.ForEach(@event =>
				{
					Exception exception = null;
					var thread = new Thread(() =>
					{
						try
						{
							ProcessLikeTheBus(@event);
						}
						catch (Exception ex)
						{
							PreserveStack.For(ex);
							exception = ex;
						}
					});
					thread.Start();
					thread.Join();
					if (exception != null)
						throw exception;
				});
		}

		[AsSystem]
		protected virtual void ProcessLikeTheBus(IEvent @event)
		{
			_processor.Process(@event);
		}
	}
}