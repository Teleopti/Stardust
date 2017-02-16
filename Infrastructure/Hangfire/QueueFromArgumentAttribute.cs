using System.Linq;
using Hangfire.Common;
using Hangfire.States;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class QueueFromArgumentAttribute : JobFilterAttribute, IElectStateFilter
	{
		private readonly string _format;

		public QueueFromArgumentAttribute()
		{
		}

		public QueueFromArgumentAttribute(string format)
		{
			_format = format;
		}

		public void OnStateElection(ElectStateContext context)
		{
			var enqueuedState = context.CandidateState as EnqueuedState;
			if (enqueuedState == null)
				return;

			var args = context.BackgroundJob.Job.Args.ToArray();
			var jobInfo = args.OfType<HangfireEventJob>().SingleOrDefault();
			var queueName = jobInfo?.QueueName ?? string.Format(_format ?? "", args);

			if (!string.IsNullOrEmpty(queueName))
				enqueuedState.Queue = queueName;

		}
	}
}