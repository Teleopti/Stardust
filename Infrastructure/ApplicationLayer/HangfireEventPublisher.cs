using System.Linq;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventPublisher : IHangfireEventPublisher
	{
		private readonly IHangfireEventClient _client;
		private readonly IJsonEventSerializer _serializer;
		private readonly IResolveEventHandlers _resolveEventHandlers;
		private readonly bool _displayNames;

		public HangfireEventPublisher(
			IHangfireEventClient client, 
			IJsonEventSerializer serializer, 
			IResolveEventHandlers resolveEventHandlers,
			IConfigReader config)
		{
			_client = client;
			_serializer = serializer;
			_resolveEventHandlers = resolveEventHandlers;
			_displayNames = config.ReadValue("HangfireDashboardDisplayNames", false);
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				var eventType = @event.GetType();
				var serialized = _serializer.SerializeEvent(@event);
				var handlers = _resolveEventHandlers.ResolveHandlersForEvent(@event).OfType<IRunOnHangfire>();

				foreach (var handler in handlers)
				{
					var handlerType = ProxyUtil.GetUnproxiedType(handler);
					var handlerTypeName = handlerType.FullName + ", " + handlerType.Assembly.GetName().Name;
					var eventTypeName = eventType.FullName + ", " + eventType.Assembly.GetName().Name;
					string displayName = null;
					if (_displayNames)
						displayName = eventType.Name + " to " + handlerType.Name;
					_client.Enqueue(displayName, eventTypeName, serialized, handlerTypeName);
				}
			}
		}
	}
}