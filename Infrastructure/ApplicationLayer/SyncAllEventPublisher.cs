﻿using System.Linq;
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

		public SyncAllEventPublisher(ResolveEventHandlers resolver)
		{
			_resolver = resolver;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				var handlerTypes = _resolver.HandlerTypesFor<IRunOnServiceBus>(@event)
					.Concat(_resolver.HandlerTypesFor<IRunOnHangfire>(@event));

				foreach (var handlerType in handlerTypes)
				{
					var handler = _resolver.HandlerFor(handlerType);
					var method = _resolver.HandleMethodFor(handlerType, @event);
					try
					{
						method.Invoke(handler, new[] { @event });
					}
					catch (TargetInvocationException e)
					{
						PreserveStack.ForInnerOf(e);
						throw e;
					}
				}
			}
		}
	}
}