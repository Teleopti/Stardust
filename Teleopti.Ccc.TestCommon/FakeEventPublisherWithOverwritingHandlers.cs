using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeEventPublisherWithOverwritingHandlers: IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;

		private readonly Dictionary<string, Type> _overwritingEventHandling = new Dictionary<string,Type>();

		public FakeEventPublisherWithOverwritingHandlers(
			ResolveEventHandlers resolver,
			CommonEventProcessor processor)
		{
			_resolver = resolver;
			_processor = processor;
		}

		public void Reset()
		{
			_overwritingEventHandling.Clear();
		}

		public void OverwriteHandler(Type eventType, Type handleType)
		{
			_overwritingEventHandling.Add(eventType.FullName, handleType);
		}

		public void Publish(params IEvent[] events)
		{
			foreach(var @event in events)
			{
				var eventType = @event.GetType().FullName;
				if (_overwritingEventHandling.ContainsKey(eventType))
				{
					_processor.Process(@event, _overwritingEventHandling[eventType]);
				}
				else
				{
#pragma warning disable 618
					var handlerTypes = _resolver.HandlerTypesFor<IRunOnServiceBus>(@event)
#pragma warning restore 618
						.Concat(_resolver.HandlerTypesFor<IRunOnHangfire>(@event))
						.Concat(_resolver.HandlerTypesFor<IRunOnStardust>(@event))
						.Concat(_resolver.HandlerTypesFor<IRunInSyncInFatClientProcess>(@event));
					foreach(var handlerType in handlerTypes)
						_processor.Process(@event,handlerType);
				}
			}
		}

	}
}