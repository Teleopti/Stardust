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
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireUtilities : ICleanHangfire, IManageFailedHangfireEvents
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

		public void TriggerRecurringJobs() => triggerRecurringJob(null);
		public void TriggerDailyRecurringJobs() => triggerRecurringJob(Cron.Daily());
		public void TriggerHourlyRecurringJobs() => triggerRecurringJob(Cron.Hourly());
		public void TriggerMinutelyRecurringJobs() => triggerRecurringJob(Cron.Minutely());

		private void triggerRecurringJob(string cron)
		{
			var jobs = _storage.Value.GetConnection().GetRecurringJobs();
			jobs
				.Where(j => cron == null || j.Cron == cron)
				.ForEach(j => { recurringJobs.Trigger(j.Id); });
		}

		public void RequeueScheduledJobs() => monitoring.ScheduledJobs(0, 100).ForEach(j => backgroundJobs.Requeue(j.Key));

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
		
		public long NumberOfEnqueuedJobs() => Queues.OrderOfPriority().Select(x => monitoring.EnqueuedCount(x)).Sum();
		public long NumberOfJobsInQueue(string name) => monitoring.EnqueuedCount(name);
		public long NumberOfFailedJobs() => monitoring.FailedCount();
		public long NumberOfScheduledJobs() => monitoring.ScheduledCount();
		public long NumberOfProcessingJobs() => monitoring.ProcessingCount();
		public long NumberOfSucceededJobs() => monitoring.SucceededListCount();
		public long SucceededFromStatistics() => monitoring.GetStatistics().Succeeded;

		public void CleanFailedJobsBefore(DateTime time)
		{
			const int batch = 100;
			var iteration = 0;

			var expiredFailed = new Dictionary<string, FailedJobDto>();
			while (true)
			{
				var failedJobs = monitoring.FailedJobs(iteration * batch, batch);
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
			var exception = failedJobsException();
			deleteJobs(() => monitoring.FailedJobs(0, 1000).Select(x => x.Key), null);
			throw exception;
		}

		public void ThrowExceptionFromAnyFailedJob()
		{
			var e = failedJobsException();
			if (e != null)
				throw e;
		}

		private AggregateException failedJobsException()
		{
			var exceptions = (
				from j in monitoring.FailedJobs(0, 10)
				select new Exception($"Hangfire job failure: {j.Value.ExceptionDetails}")
			).ToArray();
			if (exceptions.Any())
				return new AggregateException("Hangfire job has failed!", exceptions);
			return null;
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

		public void RequeueFailed(string eventName, string handlerName, string tenant) =>
			getFailedJobs(eventName, handlerName, tenant).ForEach(jobId => backgroundJobs.Requeue(jobId));

		public void DeleteFailed(string eventName, string handlerName, string tenant) =>
			getFailedJobs(eventName, handlerName, tenant).ForEach(jobId => backgroundJobs.Delete(jobId));

		private IList<string> getFailedJobs(string eventName, string handlerName, string tenant)
		{
			const int batch = 100;
			var iteration = 0;

			var failed = new Dictionary<string, FailedJobDto>();
			while (true)
			{
				var failedJobs = monitoring.FailedJobs(iteration * batch, batch);
				failedJobs
					.Where(x =>
					{
						string tenant1;
						string eventName1;
						string handlerName1;
						try
						{
							if (x.Value.Job.Args.Count == 5)
							{
								tenant1 = (string) x.Value.Job.Args[1];
								eventName1 = ((string) x.Value.Job.Args[2]).Split(',')[0];
								handlerName1 = ((string) x.Value.Job.Args[4]).Split(',')[0];
							}
							else if (x.Value.Job.Args.Count == 6)
							{
								tenant1 = (string) x.Value.Job.Args[1];
								eventName1 = ((string) x.Value.Job.Args[3]).Split(',')[0];
								handlerName1 = ((string) x.Value.Job.Args[5]).Split(',')[0];
							}
							else if (x.Value.Job.Args.Count == 7)
							{
								tenant1 = (string) x.Value.Job.Args[1];
								eventName1 = ((string) x.Value.Job.Args[4]).Split(',')[0];
								handlerName1 = ((string) x.Value.Job.Args[6]).Split(',')[0];
							}
							else
							{
								var job = x.Value.Job.Args.OfType<HangfireEventJob>().FirstOrDefault();
								if (job == null) return false;
								tenant1 = job.Tenant;
								eventName1 = job.Event.GetType().FullName;
								handlerName1 = job.HandlerTypeName.Split(',')[0];
							}
						}
						catch (Exception)
						{
							return false;
						}

						bool match = true;
						if (eventName != null)
							match = eventName1 == eventName;
						if (handlerName != null)
							match = match && handlerName1 == handlerName;
						if (tenant != null)
							match = match && tenant1 == tenant;
						return match;
					})
					.ForEach(x => failed[x.Key] = x.Value);

				if (failedJobs.Count < batch)
					break;

				iteration++;
			}

			return failed.Keys.ToList();
		}
	}
}