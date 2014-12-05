using Hangfire;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	public class HangfireEventClient : IHangfireEventClient
	{
		private readonly IBackgroundJobClient _jobClient;

		public HangfireEventClient(IBackgroundJobClient jobClient)
		{
			_jobClient = jobClient;
		}

		public void Enqueue(string displayName, string eventType, string serializedEvent)
		{
			_jobClient.Enqueue<HangfireEventProcessor>(x => x.Process(displayName, eventType, serializedEvent));
		}
	}
}