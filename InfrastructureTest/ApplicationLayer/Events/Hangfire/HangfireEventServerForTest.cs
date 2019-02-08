using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire
{
	public class HangfireEventServerForTest
	{
		private readonly HangfireEventServer _server;
		private readonly IJsonEventDeserializer _deserializer;
		private readonly PersistedTypeMapper _typeMapper;

		public HangfireEventServerForTest(HangfireEventServer server, IJsonEventDeserializer deserializer, PersistedTypeMapper typeMapper)
		{
			_server = server;
			_deserializer = deserializer;
			_typeMapper = typeMapper;
		}
		
		public void ProcessForTest(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			var eventT = Type.GetType(eventType, true);
			var @event = _deserializer.DeserializeEvent(serializedEvent, eventT) as IEvent;
			var handlerT = Type.GetType(handlerType, true);
			var handlerTypeName = _typeMapper.NameForPersistence(handlerT);
			_server.Process(displayName,
				new HangfireEventJob
				{
					DisplayName = displayName,
					Tenant = tenant,
					Event = @event,
					HandlerTypeName = handlerTypeName
				});
		}
	}
}