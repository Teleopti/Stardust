using Hangfire;
using Hangfire.Storage;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public interface ITriggerHangfireRecurringJobs
	{
		void Trigger();
	}

	public class NoTriggerHangfireRecurringJobs : ITriggerHangfireRecurringJobs
	{
		public void Trigger()
		{
		}
	}

	public class TriggerHangfireRecurringJobs : ITriggerHangfireRecurringJobs
	{
		private readonly JobStorage _storage;
		private readonly RecurringJobManager _recurringJobs;

		public TriggerHangfireRecurringJobs(JobStorage storage, RecurringJobManager recurringJobs)
		{
			_storage = storage;
			_recurringJobs = recurringJobs;
		}

		public void Trigger()
		{
			var jobs = _storage.GetConnection().GetRecurringJobs();
			jobs.ForEach(j =>
			{
				_recurringJobs.Trigger(j.Id);
			});
		}
	}
}