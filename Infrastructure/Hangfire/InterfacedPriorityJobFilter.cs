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

			var handlerType = (string)context.BackgroundJob.Job.Args[4];
			var interfaces = Type.GetType(handlerType).GetInterfaces();
			if (interfaces.Any(i => i == typeof(IRunWithHighPriority)))
				enqueuedState.Queue = Priority.High.ToString().ToLower();
			else if (interfaces.Any(i => i == typeof(IRunWithDefaultPriority)))
				enqueuedState.Queue = Priority.Default.ToString().ToLower();
			else if (interfaces.Any(i => i == typeof(IRunWithLowPriority)))
				enqueuedState.Queue = Priority.Low.ToString().ToLower();
		}
	}
}