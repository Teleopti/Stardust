﻿using System.Reflection;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class StardustEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;

		public StardustEventPublisher(
			ResolveEventHandlers resolver,
			CommonEventProcessor processor)
		{
			_resolver = resolver;
			_processor = processor;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
				foreach (var handlerType in _resolver.HandlerTypesFor<IRunOnStardust>(@event))
					_processor.Process(@event, handlerType);
		}
	}
}