using System;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventProcessor : IHangfireEventProcessor
	{
		private readonly ISyncEventPublisher _publisher;
		private readonly IJsonDeserializer _deserializer;

		public HangfireEventProcessor(ISyncEventPublisher publisher, IJsonDeserializer deserializer)
		{
			_publisher = publisher;
			_deserializer = deserializer;
		}

		public void Process(string displayName, string eventType, string serializedEvent)
		{
			var @event = _deserializer.DeserializeObject(serializedEvent, Type.GetType(eventType, true)) as IEvent;
			_publisher.Publish(@event);
		}
	}
}