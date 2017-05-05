using System;
using System.ComponentModel;
using Hangfire;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireEventServer
	{
		private readonly HangfireEventProcessor _processor;
		private readonly IJsonEventDeserializer _deserializer;

		public HangfireEventServer(
			HangfireEventProcessor processor,
			IJsonEventDeserializer deserializer)
		{
			_processor = processor;
			_deserializer = deserializer;
		}
		
		[DisplayName("{0}")]
		[QueueFromArgument]
		[AllowFailuresFromArgument(Order = 1)]
		[AttemptsFromArgument(Order = 2)]
		public void Process(string displayName, HangfireEventJob job)
		{
			_processor.Process(job.DisplayName, job.Tenant, job.Event, job.Package, job.HandlerTypeName);
		}





		[Obsolete("Backward compatibility for hangfire queue")]
		public void Process(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			var eventT = Type.GetType(eventType, true);
			var @event = _deserializer.DeserializeEvent(serializedEvent, eventT) as IEvent;
			_processor.Process(displayName, tenant, @event, null, handlerType);
		}

		[Obsolete("Backward compatibility for hangfire queue")]
		[DisplayName("{0}")]
		[QueueFromArgument("{2}")]
		[AutomaticRetry(Attempts = 10, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
		public void Process(string displayName, string tenant, string queueName, string eventType, string serializedEvent, string handlerType)
		{
			var eventT = Type.GetType(eventType, true);
			var @event = _deserializer.DeserializeEvent(serializedEvent, eventT) as IEvent;
			_processor.Process(displayName, tenant, @event, null, handlerType);
		}

		[Obsolete("Backward compatibility for hangfire queue")]
		[DisplayName("{0}")]
		[QueueFromArgument("{2}")]
		[AttemptsFromArgument("{3}")]
		public void Process(string displayName, string tenant, string queueName, int attempts, string eventType, string serializedEvent, string handlerType)
		{
			var eventT = Type.GetType(eventType, true);
			var @event = _deserializer.DeserializeEvent(serializedEvent, eventT) as IEvent;
			_processor.Process(displayName, tenant, @event, null, handlerType);
		}

	}
}