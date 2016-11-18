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

			var args = context.BackgroundJob.Job.Args;
			var jobInfo = args.OfType<HangfireEventJob>().SingleOrDefault();
			var queueName = jobInfo?.QueueName;

			if (jobInfo == null)
				queueName = string.Format(_format, (string[])args.Select(a => a as string).ToArray());

			if (!string.IsNullOrEmpty(queueName))
				enqueuedState.Queue = queueName;
		}
	}
}