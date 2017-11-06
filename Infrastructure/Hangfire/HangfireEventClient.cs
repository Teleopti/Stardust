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
		
		public void Enqueue(HangfireEventJob job)
		{
			_jobClient.Value.Enqueue<HangfireEventServer>(x => x.Process(job.DisplayName, job));
		}

		public void AddOrUpdateHourly(HangfireEventJob job)
		{
			Expression<Action<HangfireEventServer>> f = x => x.Process(job.DisplayName, job);
			_recurringJob.Value.AddOrUpdate(job.RecurringId(), Job.FromExpression(f), Cron.Hourly());
		}
		
		public void AddOrUpdateMinutely(HangfireEventJob job)
		{
			Expression<Action<HangfireEventServer>> f = x => x.Process(job.DisplayName, job);
			_recurringJob.Value.AddOrUpdate(job.RecurringId(), Job.FromExpression(f), job.RunInterval <= 1 ? Cron.Minutely() : Cron.MinuteInterval(job.RunInterval));
		}

		public void AddOrUpdateDaily(HangfireEventJob job)
		{
			Expression<Action<HangfireEventServer>> f = x => x.Process(job.DisplayName, job);
			_recurringJob.Value.AddOrUpdate(job.RecurringId(), Job.FromExpression(f), Cron.Daily());
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