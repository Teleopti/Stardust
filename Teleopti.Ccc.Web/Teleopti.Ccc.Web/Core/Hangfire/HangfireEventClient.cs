using System;
using Hangfire;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[CLSCompliant(false)]
	public class HangfireEventClient : IHangfireEventClient
	{
		private readonly Lazy<IBackgroundJobClient> _jobClient;

		public HangfireEventClient(Lazy<IBackgroundJobClient> jobClient)
		{
			_jobClient = jobClient;
		}

		public void Enqueue(string displayName, string eventType, string serializedEvent, string handlerType)
		{
			_jobClient.Value.Enqueue<HangfireEventServer>(x => x.Process(displayName, eventType, serializedEvent, handlerType));
		}
	}
}