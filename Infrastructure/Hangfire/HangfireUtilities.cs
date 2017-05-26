using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Castle.Core.Internal;
using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireUtilities : ICleanHangfire
	{
		private readonly Lazy<JobStorage> _storage;
		private readonly Lazy<IBackgroundJobClient> _backgroundJobs;
		private readonly Lazy<RecurringJobManager> _recurringJobs;

		public HangfireUtilities(
			Lazy<JobStorage> storage,
			Lazy<IBackgroundJobClient> backgroundJobs,
			Lazy<RecurringJobManager> recurringJobs)
		{
			_storage = storage;
			_backgroundJobs = backgroundJobs;
			_recurringJobs = recurringJobs;
		}

		private IMonitoringApi monitoring => _storage.Value.GetMonitoringApi();
		private IBackgroundJobClient backgroundJobs => _backgroundJobs.Value;
		private RecurringJobManager recurringJobs => _recurringJobs.Value;

		public void TriggerReccuringJobs()
		{
			triggerRecurringJob(null);
		}

		public void TriggerDailyRecurringJobs()
		{
			triggerRecurringJob(Cron.Daily());
		}

		public void TriggerHourlyRecurringJobs()
		{
			triggerRecurringJob(Cron.Hourly());
		}

		public void TriggerMinutelyRecurringJobs()
		{
			triggerRecurringJob(Cron.Minutely());
		}

		private void triggerRecurringJob(string cron)
		{
			var jobs = _storage.Value.GetConnection().GetRecurringJobs();
			jobs
				.Where(j => cron == null || j.Cron == cron)
				.ForEach(j =>
				{
					recurringJobs.Trigger(j.Id);
				});
		}

		public void RequeueScheduledJobs()
		{
			monitoring
				.ScheduledJobs(0, 100)
				.ForEach(j => backgroundJobs.Requeue(j.Key));
		}

		public void CleanQueue()
		{
			foreach (var queueName in Queues.OrderOfPriority())
				deleteJobs(() => monitoring.EnqueuedJobs(queueName, 0, 10).Select(x => x.Key), EmulateWorkerIteration);
			deleteJobs(() => monitoring.ScheduledJobs(0, 10).Select(x => x.Key), null);
			deleteJobs(() => monitoring.FailedJobs(0, 10).Select(x => x.Key), null);
			deleteJobs(() => monitoring.SucceededJobs(0, 10).Select(x => x.Key), null);
		}

		private void deleteJobs(Func<IEnumerable<string>> ids, Action afterDelete)
		{
			var jobs = ids.Invoke();
			while (jobs.Any())
			{
				jobs.ForEach(j =>
				{
					backgroundJobs.Delete(j);
					afterDelete?.Invoke();
				});
				jobs = ids.Invoke();
			}
		}

		public void CancelQueue()
		{
			foreach (var queueName in Queues.OrderOfPriority())
			{
				monitoring
				.EnqueuedJobs(queueName, 0, 1000)
				.ForEach(j => backgroundJobs.Delete(j.Key));
			}

			monitoring
				.ScheduledJobs(0, 1000)
				.ForEach(j => backgroundJobs.Delete(j.Key));
		}

		public long NumberOfJobsInQueue(string name)
		{
			return monitoring.EnqueuedCount(name);
		}

		public long NumberOfEnqueuedJobs()
		{
			return Queues.OrderOfPriority()
				.Select(x => monitoring.EnqueuedCount(x))
				.Sum();
		}

		public long NumberOfFailedJobs()
		{
			return monitoring.FailedCount();
		}

		public long NumberOfScheduledJobs()
		{
			return monitoring.ScheduledCount();
		}

		public long NumberOfProcessingJobs()
		{
			return monitoring.ProcessingCount();
		}

		public long NumberOfSucceededJobs()
		{
			return monitoring.SucceededListCount();
		}

		public void CleanFailedJobsBefore(DateTime time)
		{
			const int batch = 100;
			var iteration = 0;

			var expiredFailed = new Dictionary<string, FailedJobDto>();
			while (true)
			{
				var failedJobs = monitoring.FailedJobs(iteration*batch, batch);
				 failedJobs
					.Where(x => x.Value.FailedAt.HasValue && x.Value.FailedAt < time)
					.ForEach(x => expiredFailed[x.Key] = x.Value);
				
				if (failedJobs.Count < batch)
					break;

				iteration++;
			}

			expiredFailed.ForEach(j => backgroundJobs.Delete(j.Key));
		}

		[TestLog]
		public virtual void WaitForQueue()
		{
			while (true)
			{
				long enqueuedCount = 0;
				long fetchedCount = 0;
				foreach (var queueName in Queues.OrderOfPriority())
				{
					enqueuedCount += monitoring.EnqueuedCount(queueName);
					fetchedCount += monitoring.FetchedCount(queueName);
				}
				var scheduledCount = monitoring.ScheduledCount();
				var processingCount = monitoring.ProcessingCount();

				// all is well
				if (enqueuedCount + fetchedCount + scheduledCount + processingCount == 0)
					break;

				// requeue any scheduled retries if queue is empty
				if (enqueuedCount == 0 && scheduledCount > 0)
					RequeueScheduledJobs();

				Thread.Sleep(20);
			}

			if (monitoring.FailedCount() <= 0) return;

			// booom!
			var exceptions = (
				from j in monitoring.FailedJobs(0, 10)
				select new Exception($"Hangfire job failure: {j.Value.ExceptionDetails}")
			).ToArray();
			deleteJobs(() => monitoring.FailedJobs(0, 1000).Select(x => x.Key), null);
			throw new AggregateException("Hangfire job has failed!", exceptions);
		}






		public void EmulateWorkerIteration()
		{
			// will hang if nothing to work with
			if (NumberOfEnqueuedJobs() > 0)
				new Worker(Queues.OrderOfPriority().ToArray()).Execute(
					new BackgroundProcessContext(
						"fake server",
						_storage.Value,
						new Dictionary<string, object>(),
						new CancellationToken()
					));
		}

	}
}