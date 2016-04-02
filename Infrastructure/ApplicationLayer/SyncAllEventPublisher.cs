using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncAllEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;

		public SyncAllEventPublisher(
			ResolveEventHandlers resolver,
			CommonEventProcessor processor)
		{
			_resolver = resolver;
			_processor = processor;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				var handlerTypes = _resolver.HandlerTypesFor<IRunOnServiceBus>(@event)
					.Concat(_resolver.HandlerTypesFor<IRunOnHangfire>(@event));
				foreach (var handlerType in handlerTypes)
					_processor.Process(@event, handlerType);
			}
		}

	}
}