using System;
using System.Linq;
using Castle.DynamicProxy;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventProcessor : IHangfireEventProcessor
	{
		private readonly IJsonDeserializer _deserializer;
		private readonly IResolveEventHandlers _resolver;

		public HangfireEventProcessor(IJsonDeserializer deserializer, IResolveEventHandlers resolver)
		{
			_deserializer = deserializer;
			_resolver = resolver;
		}

		public void Process(string displayName, string eventType, string serializedEvent, string handlerType)
		{
			var handlerT = Type.GetType(handlerType, true);
			var eventT = Type.GetType(eventType, true);
			var @event = _deserializer.DeserializeObject(serializedEvent, eventT) as IEvent;
			var handlers = _resolver.ResolveHandlersForEvent(@event).Cast<object>();
			var publishTo = handlers.Single(o => ProxyUtil.GetUnproxiedType(o) == handlerT);
			new SyncPublishToSingleHandler(publishTo).Publish(@event);
		}
	}
}