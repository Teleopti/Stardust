using System;
using Hangfire;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	[CLSCompliant(false)]
	public class HangfireEventClient : IHangfireEventClient
	{
		private readonly Lazy<IBackgroundJobClient> _jobClient;

		public HangfireEventClient(Lazy<IBackgroundJobClient> jobClient)
		{
			_jobClient = jobClient;
		}

		public void Enqueue(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			_jobClient.Value.Enqueue<HangfireEventServer>(x => x.Process(displayName, tenant, eventType, serializedEvent, handlerType));

			//RecurringJob.AddOrUpdate<HangfireEventServer>("id", x => x.Process(displayName, tenant, eventType, serializedEvent, handlerType), Cron.Hourly);

			//var a = new RecurringJobManager();
			//Expression<Action<HangfireEventServer>> f = x => x.Process(displayName, tenant, eventType, serializedEvent, handlerType);
			//a.AddOrUpdate("id", Job.FromExpression(f), Cron.Hourly());

		}

		public void AddOrUpdateRecurring(string displayName, string id, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			throw new NotImplementedException();
		}
	}
}