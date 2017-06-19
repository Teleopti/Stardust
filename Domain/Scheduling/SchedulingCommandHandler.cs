using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingCommandHandler
	{
		private readonly SchedulingEventHandler _schedulingEventHandler;

		public SchedulingCommandHandler(SchedulingEventHandler schedulingEventHandler)
		{
			_schedulingEventHandler = schedulingEventHandler;
		}

		public void Execute(SchedulingCommand schedulingCommand,
			SchedulingOptions schedulingOptions,
			ISchedulingCallback schedulingCallback,
			ISchedulingProgress backgroundWorker,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var selectedAgents = schedulingCommand.Agents;
			var selectedPeriod = schedulingCommand.Period;

			var @event = new SchedulingWasOrdered
			{
				AgentsToOptimize = selectedAgents.Select(x => x.Id.Value),
				StartDate = selectedPeriod.StartDate,
				EndDate = selectedPeriod.EndDate
			};

			//should use IEventPublisher instead
			_schedulingEventHandler.HandleEvent(@event, schedulingOptions, schedulingCallback, backgroundWorker,
				dayOffOptimizationPreferenceProvider);
		}
	}
}


