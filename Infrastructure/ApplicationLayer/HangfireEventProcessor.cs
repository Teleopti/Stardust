using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventProcessor
	{
		private readonly IJsonEventDeserializer _deserializer;
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;
		private readonly TimeSpan _timeout;

		public HangfireEventProcessor(
			IJsonEventDeserializer deserializer,
			ResolveEventHandlers resolver,
			CommonEventProcessor processor,
			IConfigReader config)
		{
			_deserializer = deserializer;
			_resolver = resolver;
			_processor = processor;
			_timeout = TimeSpan.FromSeconds(config.ReadValue("HangfireJobTimeoutSeconds", 15*60));
		}

		public void Process(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			var eventT = Type.GetType(eventType, true);
			var @event = _deserializer.DeserializeEvent(serializedEvent, eventT) as IEvent;
			Process(displayName, tenant, @event, handlerType);
		}

		public void Process(string displayName, string tenant, IEvent @event, string handlerType)
		{
			var handlerT = Type.GetType(handlerType, true);
			var handlers = _resolver.HandlerTypesFor<IRunOnHangfire>(@event);

			var publishTo = handlers.Single(o => o == handlerT);

			runWithTimeout(tenant, @event, publishTo);
		}

		private void runWithTimeout(string tenant, IEvent @event, Type publishTo)
		{
			Exception exception = null;
			var thread = new Thread(() =>
			{
				try
				{
					_processor.Process(tenant, @event, publishTo);
				}
				catch (Exception e)
				{
					exception = e;
				}
			});
			thread.Start();
			thread.Join(_timeout);
			if (thread.IsAlive)
			{
				thread.Abort();
				while (thread.IsAlive)
					Thread.Sleep(100);
				throw new TimeoutException($"Hangfire job did not finish within {_timeout}", exception);
			}
			if (exception != null)
				ExceptionDispatchInfo.Capture(exception).Throw();
		}
	}
}