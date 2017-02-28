using System;
using System.ComponentModel;
using Hangfire;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireEventServer
	{
		private readonly HangfireEventProcessor _processor;

		public HangfireEventServer(HangfireEventProcessor processor)
		{
			_processor = processor;
		}
		
		[Obsolete("backward compatible")]
		public void Process(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			_processor.Process(displayName, tenant, eventType, serializedEvent, handlerType);
		}

		[DisplayName("{0}")]
		[QueueFromArgument("{2}")]
		[AutomaticRetry(Attempts = 10, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
		public void Process(string displayName, string tenant, string queueName, string eventType, string serializedEvent, string handlerType)
		{
			_processor.Process(displayName, tenant, eventType, serializedEvent, handlerType);
		}

		[DisplayName("{0}")]
		[QueueFromArgument("{2}")]
		[AttemptsFromArgument("{3}")]
		public void Process(string displayName, string tenant, string queueName, int attempts, string eventType, string serializedEvent, string handlerType)
		{
			_processor.Process(displayName, tenant, eventType, serializedEvent, handlerType);
		}

		[DisplayName("{0}")]
		[QueueFromArgument]
		[AllowFailuresFromArgument(Order = 1)]
		[AttemptsFromArgument(Order = 2)]
		public void Process(string displayName, HangfireEventJob job)
		{
			_processor.Process(job.DisplayName, job.Tenant, job.Event, job.HandlerTypeName);
		}
	}
}