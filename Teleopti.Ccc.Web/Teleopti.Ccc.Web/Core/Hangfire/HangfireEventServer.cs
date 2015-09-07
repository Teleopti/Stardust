using System.ComponentModel;
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
		public void Process(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			_processor.Process(displayName, tenant, eventType, serializedEvent, handlerType);
		}
	}
}