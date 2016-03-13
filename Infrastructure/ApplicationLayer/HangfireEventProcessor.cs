using System;
using System.Linq;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventProcessor
	{
		private readonly IJsonEventDeserializer _deserializer;
		private readonly ResolveEventHandlers _resolver;
		private readonly IDataSourceScope _dataSourceScope;

		public HangfireEventProcessor(
			IJsonEventDeserializer deserializer,
			ResolveEventHandlers resolver,
			IDataSourceScope dataSourceScope)
		{
			_deserializer = deserializer;
			_resolver = resolver;
			_dataSourceScope = dataSourceScope;
		}

		public void Process(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			var handlerT = Type.GetType(handlerType, true);
			var eventT = Type.GetType(eventType, true);
			var @event = _deserializer.DeserializeEvent(serializedEvent, eventT) as IEvent;
			var handlers = _resolver.ResolveHangfireHandlersForEvent(@event);
			var publishTo = handlers.Single(o => ProxyUtil.GetUnproxiedType(o) == handlerT);

			using (_dataSourceScope.OnThisThreadUse(tenant))
				new SyncPublishTo(_resolver, publishTo).Publish(@event);
		}
	}
}