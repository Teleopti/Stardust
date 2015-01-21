using Castle.DynamicProxy;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventPublisher : IHangfireEventPublisher
	{
		private readonly IHangfireEventClient _client;
		private readonly IJsonSerializer _serializer;
		private readonly IResolveEventHandlers _resolveEventHandlers;

		public HangfireEventPublisher(IHangfireEventClient client, IJsonSerializer serializer, IResolveEventHandlers resolveEventHandlers)
		{
			_client = client;
			_serializer = serializer;
			_resolveEventHandlers = resolveEventHandlers;
		}

		public void Publish(IEvent @event)
		{
			var type = @event.GetType();
			var serialized = _serializer.SerializeObject(@event);
			var handlers = _resolveEventHandlers.ResolveHandlersForEvent(@event);

			foreach (var handler in handlers)
			{
				var handlerType = ProxyUtil.GetUnproxiedType(handler);
				_client.Enqueue(type.Name  + " to " + handlerType.Name, type.AssemblyQualifiedName, serialized, handlerType.AssemblyQualifiedName);
			}
		}
	}
}