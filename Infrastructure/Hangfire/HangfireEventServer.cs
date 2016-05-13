using System;
using System.ComponentModel;
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
		
		[Obsolete("backward compatible, don't delete it j�gej. /erik")]
		public void Process(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			_processor.Process(displayName, tenant, eventType, serializedEvent, handlerType);
		}

		[DisplayName("{0}")]
		public void Process(string displayName, string tenant, string queueName, string eventType, string serializedEvent, string handlerType)
		{
			_processor.Process(displayName, tenant, eventType, serializedEvent, handlerType);
		}
	}
}