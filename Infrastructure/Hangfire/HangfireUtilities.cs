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
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireUtilities : ICleanHangfire
	{
		private readonly Lazy<JobStorage> _storage;
		private readonly Lazy<IBackgroundJobClient> _backgroundJobs;
		private readonly Lazy<RecurringJobManager> _recurringJobs;
		private readonly WithAnalyticsUnitOfWork _withUnitOfWork;

		public HangfireUtilities(
			Lazy<JobStorage> storage,
			Lazy<IBackgroundJobClient> backgroundJobs,
			Lazy<RecurringJobManager> recurringJobs,
			WithAnalyticsUnitOfWork withUnitOfWork)
		{
			_storage = storage;
			_backgroundJobs = backgroundJobs;
			_recurringJobs = recurringJobs;
			_withUnitOfWork = withUnitOfWork;
		}

		private IMonitoringApi monitoring => _storage.Value.GetMonitoringApi();
		private IBackgroundJobClient backgroundJobs => _backgroundJobs.Value;
		private RecurringJobManager recurringJobs => _recurringJobs.Value;

		public void TriggerReccuringJobs()
		{
			var jobs = _storage.Value.GetConnection().GetRecurringJobs();
			jobs.ForEach(j =>
			{
				recurringJobs.Trigger(j.Id);
			});
		}

		public void CleanQueue()
		{
			foreach (var queueName in Queues.OrderOfPriority())
				deleteJobs(() => monitoring.EnqueuedJobs(queueName, 0, 10).Select(x => x.Key), WorkerIteration);
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

		public void WorkerIteration()
		{
			// will hang if nothing to work with
			var count = Queues.OrderOfPriority()
				.Select(x => monitoring.EnqueuedCount(x))
				.Sum();
			if (count > 0)
				new Worker(Queues.OrderOfPriority().ToArray()).Execute(
					new BackgroundProcessContext(
						"fake server",
						_storage.Value,
						new Dictionary<string, object>(),
						new CancellationToken()
					));
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

		public void RequeueScheduledJobs()
		{
			monitoring
				.ScheduledJobs(0, 100)
				.ForEach(j => backgroundJobs.Requeue(j.Key));
		}

		public void DeleteQueues()
		{
			_withUnitOfWork.Do(uow =>
			{
				uow.Current().Session()
					.CreateSQLQuery("DELETE FROM HangFire.Job")
					.ExecuteUpdate();
			});
		}
	}
}