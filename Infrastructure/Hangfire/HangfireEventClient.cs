using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireEventClient : IHangfireEventClient
	{
		private readonly Lazy<IBackgroundJobClient> _jobClient;
		private readonly Lazy<RecurringJobManager> _recurringJob;
		private readonly Lazy<JobStorage> _storage;

		public HangfireEventClient(
			Lazy<IBackgroundJobClient> jobClient,
			Lazy<RecurringJobManager> recurringJob,
			Lazy<JobStorage> storage)
		{
			_jobClient = jobClient;
			_recurringJob = recurringJob;
			_storage = storage;
		}

		public void Enqueue(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			_jobClient.Value.Enqueue<HangfireEventServer>(x => x.Process(displayName, tenant, eventType, serializedEvent, handlerType));
		}

		public void AddOrUpdateHourly(string displayName, string id, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			Expression<Action<HangfireEventServer>> f = x => x.Process(displayName, tenant, eventType, serializedEvent, handlerType);
			_recurringJob.Value.AddOrUpdate(id, Job.FromExpression(f), Cron.Hourly());
		}

		public void RemoveIfExists(string id)
		{
			_recurringJob.Value.RemoveIfExists(id);
		}

		public IEnumerable<string> GetRecurringJobIds()
		{
			return _storage.Value.GetConnection()
				.GetRecurringJobs()
				.Select(x => x.Id)
				.ToArray();
		}
	}
	
}