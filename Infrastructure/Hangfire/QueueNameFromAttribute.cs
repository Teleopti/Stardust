using System.Linq;
using Hangfire.Common;
using Hangfire.States;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class QueueNameFromAttribute : JobFilterAttribute, IElectStateFilter
	{
		private readonly string _queueName;

		public QueueNameFromAttribute(string queueName)
		{
			_queueName = queueName;
		}

		public void OnStateElection(ElectStateContext context)
		{
			var enqueuedState = context.CandidateState as EnqueuedState;
			if (enqueuedState == null)
				return;

			var args = context.BackgroundJob.Job.Args.Select(a => a as string).ToArray();
			var queueName = string.Format(_queueName, args);
			if (!string.IsNullOrEmpty(queueName))
				enqueuedState.Queue = queueName;
		}
	}
}