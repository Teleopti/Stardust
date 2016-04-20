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
	public class SyncAllEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;
		private readonly ServiceBusEventProcessor _serviceBusEventProcessor;


		public SyncAllEventPublisher(
			ResolveEventHandlers resolver,
			CommonEventProcessor processor,
			ServiceBusEventProcessor serviceBusEventProcessor)
		{
			_resolver = resolver;
			_processor = processor;
			_serviceBusEventProcessor = serviceBusEventProcessor;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				var handlerTypes = _resolver.HandlerTypesFor<IRunOnHangfire>(@event);
				foreach (var handlerType in handlerTypes)
				{
					Exception exception = null;
					var thread = new Thread(() =>
					{
						try
						{
							_processor.Process(@event, handlerType);
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
					
				}
			}
			

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
			_serviceBusEventProcessor.Process(@event);
		}
	}


}