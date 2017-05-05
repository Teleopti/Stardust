using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventProcessor
	{
		private readonly IJsonEventDeserializer _deserializer;
		private readonly CommonEventProcessor _processor;

		public HangfireEventProcessor(
			IJsonEventDeserializer deserializer,
			CommonEventProcessor processor)
		{
			_deserializer = deserializer;
			_processor = processor;
		}

		[Obsolete("backward compatible")]
		public void Process(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			var eventT = Type.GetType(eventType, true);
			var @event = _deserializer.DeserializeEvent(serializedEvent, eventT) as IEvent;
			Process(displayName, tenant, @event, null, handlerType);
		}

		public void Process(string displayName, string tenant, IEvent @event, IEnumerable<IEvent> package, string handlerType)
		{
			var handlerT = Type.GetType(handlerType, true);
			Process(displayName, tenant, @event, package, handlerT);
		}

		public void Process(string displayName, string tenant, IEvent @event, IEnumerable<IEvent> package, Type handlerType)
		{
			_processor.Process(tenant, @event, package, handlerType);
		}
	}
}