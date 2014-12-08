using System.ComponentModel;
using Hangfire;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	public class HangfireEventServer
	{
		private readonly IHangfireEventProcessor _processor;

		public HangfireEventServer(IHangfireEventProcessor processor)
		{
			_processor = processor;
		}

		[DisplayName("{0}")]
		[AutomaticRetry(Attempts = 0)]
		[DisableConcurrentExecution(timeoutInSeconds: 20)]
		public void Process(string displayName, string eventType, string serializedEvent)
		{
			_processor.Process(displayName, eventType, serializedEvent);
		}
	}
}