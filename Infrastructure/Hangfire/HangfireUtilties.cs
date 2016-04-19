using System;
using System.Threading;
using Hangfire;
using Hangfire.Storage;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireUtilties
	{
		private readonly IBackgroundJobClient _backgroundJobs;
		private readonly IMonitoringApi _monitoring;

		public HangfireUtilties(JobStorage storage, IBackgroundJobClient backgroundJobs)
		{
			_backgroundJobs = backgroundJobs;
			_monitoring = storage.GetMonitoringApi();
		}

		public void CancelQueue()
		{
			foreach (var queueName in QueueNames.From<Priority>())
			{
				_monitoring
				.EnqueuedJobs(queueName, 0, 100)
				.ForEach(j => _backgroundJobs.Delete(j.Key));
			}
			
			_monitoring
				.ScheduledJobs(0, 100)
				.ForEach(j => _backgroundJobs.Delete(j.Key));
		}

		[LogTime]
		public virtual void WaitForQueue()
		{
			while (true)
			{
				long enqueuedCount = 0;
				long fetchedCount = 0;
				foreach (var queueName in QueueNames.From<Priority>())
				{
					enqueuedCount += _monitoring.EnqueuedCount(queueName);
					fetchedCount += _monitoring.FetchedCount(queueName);
				}
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
    }
}