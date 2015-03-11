using System;
using System.Linq;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Infrastructure.DistributedLock;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventProcessor : IHangfireEventProcessor
	{
		private readonly IJsonDeserializer _deserializer;
		private readonly IResolveEventHandlers _resolver;
		private readonly IDistributedLockAcquirer _distributedLockAcquirer;

		public HangfireEventProcessor(
			IJsonDeserializer deserializer, 
			IResolveEventHandlers resolver, 
			IDistributedLockAcquirer distributedLockAcquirer)
		{
			_deserializer = deserializer;
			_resolver = resolver;
			_distributedLockAcquirer = distributedLockAcquirer;
		}

		public void Process(string displayName, string eventType, string serializedEvent, string handlerType)
		{
			var handlerT = Type.GetType(handlerType, true);
			var eventT = Type.GetType(eventType, true);
			var @event = _deserializer.DeserializeObject(serializedEvent, eventT) as IEvent;
			var handlers = _resolver.ResolveHandlersForEvent(@event).Cast<object>();
			var publishTo = handlers.Single(o => ProxyUtil.GetUnproxiedType(o) == handlerT);
			using (_distributedLockAcquirer.LockForTypeOf(publishTo))
			{
				new SyncPublishToSingleHandler(publishTo).Publish(@event);
			}
		}
	}
}