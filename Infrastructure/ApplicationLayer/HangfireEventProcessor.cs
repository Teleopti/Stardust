using System;
using System.Linq;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventProcessor : IHangfireEventProcessor
	{
		private readonly IJsonEventDeserializer _deserializer;
		private readonly IResolveEventHandlers _resolver;
		private readonly IDistributedLockAcquirer _distributedLockAcquirer;
		private readonly IDataSourceScope _dataSourceScope;

		public HangfireEventProcessor(
			IJsonEventDeserializer deserializer,
			IResolveEventHandlers resolver,
			IDistributedLockAcquirer distributedLockAcquirer,
			IDataSourceScope dataSourceScope)
		{
			_deserializer = deserializer;
			_resolver = resolver;
			_distributedLockAcquirer = distributedLockAcquirer;
			_dataSourceScope = dataSourceScope;
		}

		public void Process(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			var handlerT = Type.GetType(handlerType, true);
			var eventT = Type.GetType(eventType, true);
			var @event = _deserializer.DeserializeEvent(serializedEvent, eventT) as IEvent;
			var handlers = _resolver.ResolveHandlersForEvent(@event);
			var publishTo = handlers.Single(o => ProxyUtil.GetUnproxiedType(o) == handlerT);

			using (_dataSourceScope.OnThisThreadUse(tenant))
			using (_distributedLockAcquirer.LockForTypeOf(publishTo))
				new SyncPublishTo(publishTo).Publish(@event);
		}
	}
}