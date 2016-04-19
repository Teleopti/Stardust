using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventProcessor
	{
		private readonly IJsonEventDeserializer _deserializer;
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;

		public HangfireEventProcessor(
			IJsonEventDeserializer deserializer,
			ResolveEventHandlers resolver,
			CommonEventProcessor processor)
		{
			_deserializer = deserializer;
			_resolver = resolver;
			_processor = processor;
		}

		public void Process(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			var handlerT = Type.GetType(handlerType, true);
			var eventT = Type.GetType(eventType, true);
			var @event = _deserializer.DeserializeEvent(serializedEvent, eventT) as IEvent;
			var handlers = _resolver.HandlerTypesFor<IRunOnHangfire>(@event);

			var publishTo = handlers.Single(o => o == handlerT);

			_processor.Process(tenant, @event, publishTo);
		}
	}
}