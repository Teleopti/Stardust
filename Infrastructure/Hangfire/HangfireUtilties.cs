using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Hangfire;
using Hangfire.States;
using Hangfire.Storage;
using NHibernate.Util;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	[CLSCompliant(false)]
	public class HangfireUtilties
	{
		private readonly JobStorage _storage;
		private readonly RecurringJobManager _recurringJobs;
		private readonly IBackgroundJobClient _backgroundJobs;
		private readonly IMonitoringApi _monitoring;

		public HangfireUtilties(JobStorage storage, RecurringJobManager recurringJobs, IBackgroundJobClient backgroundJobs)
		{
			_storage = storage;
			_recurringJobs = recurringJobs;
			_backgroundJobs = backgroundJobs;
			_monitoring = storage.GetMonitoringApi();
		}

		public void CancelQueue()
		{
			_monitoring
				.EnqueuedJobs(EnqueuedState.DefaultQueue, 0, 100)
				.ForEach(j => _backgroundJobs.Delete(j.Key));
			_monitoring
				.ScheduledJobs(0, 100)
				.ForEach(j => _backgroundJobs.Delete(j.Key));
		}

		public void WaitForQueue()
		{
			while (true)
			{
				var enqueuedCount = _monitoring.EnqueuedCount(EnqueuedState.DefaultQueue);
				var fetchedCount = _monitoring.FetchedCount(EnqueuedState.DefaultQueue);
				var scheduledCount = _monitoring.ScheduledCount();
				var failedCount = _monitoring.FailedCount();

				// all is well
				if (enqueuedCount + fetchedCount + scheduledCount + failedCount == 0)
					break;

				// booom!
				if (failedCount > 0)
					throw new Exception("Hangfire job has failed!");

				// requeue any scheduled retries
				if (scheduledCount > 0)
				{
					_monitoring
						.ScheduledJobs(0, 100)
						.ForEach(j => _backgroundJobs.Requeue(j.Key));
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