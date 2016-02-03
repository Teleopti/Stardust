using System;
using System.Threading;
using Hangfire;
using Hangfire.States;
using Hangfire.Storage;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	[CLSCompliant(false)]
	public class HangfireUtilties
	{
		private readonly JobStorage _storage;
		private readonly RecurringJobManager _recurringJobs;

		public HangfireUtilties(JobStorage storage, RecurringJobManager recurringJobs)
		{
			_storage = storage;
			_recurringJobs = recurringJobs;
		}

		public void CancelQueue()
		{
			var monitoring = _storage.GetMonitoringApi();
			var jobs = monitoring.EnqueuedJobs(EnqueuedState.DefaultQueue, 0, 500);
			jobs.ForEach(j => BackgroundJob.Delete(j.Key));
		}

		public void WaitForQueue()
		{
			var monitoring = _storage.GetMonitoringApi();
			while (true)
			{
				if (monitoring.EnqueuedCount(EnqueuedState.DefaultQueue) == 0 &&
					monitoring.FetchedCount(EnqueuedState.DefaultQueue) == 0)
				{
					break;
				}
				Thread.Sleep(20);
			}
		}

		public void TriggerAllRecurringJobs()
		{
			var jobs = _storage.GetConnection().GetRecurringJobs();
			jobs.ForEach(j =>
			{
				_recurringJobs.Trigger(j.Id);
			});
		}

	}
}