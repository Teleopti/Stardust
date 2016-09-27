using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Castle.Core.Internal;
using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireUtilities : ICleanHangfire
	{
		private readonly JobStorage _storage;
		private readonly IBackgroundJobClient _backgroundJobs;
		private readonly RecurringJobManager _recurringJobs;
		private readonly IMonitoringApi _monitoring;

		public HangfireUtilities(
			IJobStorageWrapper storage,
			IBackgroundJobClient backgroundJobs,
			RecurringJobManager recurringJobs)
		{
			_storage = storage.GetJobStorage();
			_backgroundJobs = backgroundJobs;
			_recurringJobs = recurringJobs;
			_monitoring = _storage.GetMonitoringApi();
		}

		public void TriggerReccuringJobs()
		{
			var jobs = _storage.GetConnection().GetRecurringJobs();
			jobs.ForEach(j =>
			{
				_recurringJobs.Trigger(j.Id);
			});
		}

		public void CleanQueue()
		{
			foreach (var queueName in Queues.OrderOfPriority())
			{
				var enqueued = _monitoring.EnqueuedJobs(queueName, 0, 1000);
				while (enqueued.Count > 0)
				{
					enqueued.ForEach(j =>
					{
						_backgroundJobs.Delete(j.Key);
						// fake a worker processing the deleted job will remove it from the job queue
						new Worker().Execute(
							new BackgroundProcessContext(
								"fake server",
								JobStorage.Current,
								new Dictionary<string, object>(),
								new CancellationToken()
							));
					});
					enqueued = _monitoring.EnqueuedJobs(queueName, 0, 1000);
				}
			}
			
		}

		public void CancelQueue()
		{
			foreach (var queueName in Queues.OrderOfPriority())
			{
				_monitoring
				.EnqueuedJobs(queueName, 0, 1000)
				.ForEach(j => _backgroundJobs.Delete(j.Key));
			}

			_monitoring
				.ScheduledJobs(0, 1000)
				.ForEach(j => _backgroundJobs.Delete(j.Key));
		}

		public long NumberOfJobsInQueue(string name)
		{
			return _monitoring.EnqueuedCount(name);
		}

		public long NumberOfFailedJobs()
		{
			return _monitoring.FailedCount();
		}

		public void CleanFailedJobsBefore(DateTime time)
		{
			const int batch = 100;
			var iteration = 0;

			var expiredFailed = new Dictionary<string, FailedJobDto>();
			while (true)
			{
				var failedJobs = _monitoring.FailedJobs(iteration*batch, batch);
				 failedJobs
					.Where(x => x.Value.FailedAt.HasValue && x.Value.FailedAt < time)
					.ForEach(x => expiredFailed[x.Key] = x.Value);
				
				if (failedJobs.Count < batch)
					break;

				iteration++;
			}

			expiredFailed.ForEach(j => _backgroundJobs.Delete(j.Key));
		}

		[LogTime]
		public virtual void WaitForQueue()
		{
			while (true)
			{
				long enqueuedCount = 0;
				long fetchedCount = 0;
				foreach (var queueName in Queues.OrderOfPriority())
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

				// requeue any scheduled retries if queue is empty
				if (enqueuedCount == 0 && scheduledCount > 0)
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