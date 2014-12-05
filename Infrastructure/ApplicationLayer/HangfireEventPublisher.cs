using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventPublisher : IHangfireEventPublisher
	{
		private readonly IHangfireEventClient _client;
		private readonly IJsonSerializer _serializer;

		public HangfireEventPublisher(IHangfireEventClient client, IJsonSerializer serializer)
		{
			_client = client;
			_serializer = serializer;
		}

		public void Publish(IEvent @event)
		{
			var type = @event.GetType();
			var serialized = _serializer == null ? null : _serializer.SerializeObject(@event);
			_client.Enqueue(type.Name, type.AssemblyQualifiedName, serialized);
		}
	}

}