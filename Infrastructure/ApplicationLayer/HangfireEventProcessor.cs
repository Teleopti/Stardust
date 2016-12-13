using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
		private readonly IConfigReader _config;

		public HangfireEventProcessor(
			IJsonEventDeserializer deserializer,
			ResolveEventHandlers resolver,
			CommonEventProcessor processor,
			IConfigReader config)
		{
			_deserializer = deserializer;
			_resolver = resolver;
			_processor = processor;
			_config = config;
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

			runFor(tenant, @event, publishTo, TimeSpan.FromSeconds(_config.ReadValue("HangfireJobTimeoutSeconds", 15 * 60)));
		}

		private void runFor(string tenant, IEvent @event, Type publishTo, TimeSpan timeout)
		{
			var task = Task.Run(() =>
			{
				_processor.Process(tenant, @event, publishTo);
			});

			var delay = Task.Delay(timeout);
			var timeoutTask = Task.WhenAny(task, delay);
			if (delay == timeoutTask.GetAwaiter().GetResult())
				throw new TimeoutException();
			timeoutTask.Result.GetAwaiter().GetResult();

			//if (!task.Wait(timeout))
			//	throw new TimeoutException();
		}

		public void Process(string tenant, IEvent @event, Type handlerType)
		{
			_processor.Process(tenant, @event, handlerType);
		}

	}
}