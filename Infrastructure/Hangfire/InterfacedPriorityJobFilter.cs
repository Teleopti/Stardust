using System;
using System.Linq;
using Hangfire.Common;
using Hangfire.States;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class InterfacedPriorityJobFilter : IJobFilter, IElectStateFilter
	{
		public bool AllowMultiple
		{
			get { return false; }
		}

		public int Order
		{
			get { return -1; }
		}

		public void OnStateElection(ElectStateContext context)
		{
			var enqueuedState = context.CandidateState as EnqueuedState;
			if (enqueuedState == null)
				return;

			var queueName = (string)context.BackgroundJob.Job.Args[2];
			enqueuedState.Queue = queueName;
		}
	}
}