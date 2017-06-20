using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public void Execute(SchedulingCommand schedulingCommand, ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences, bool runWeeklyRestSolver,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var @event = new SchedulingWasOrdered
			{
				AgentsToSchedule = schedulingCommand.AgentsToSchedule.Select(x => x.Id.Value),
				StartDate = schedulingCommand.Period.StartDate,
				EndDate = schedulingCommand.Period.EndDate
			};
			_schedulingEventHandler.HandleEvent(@event, schedulingOptions, schedulingCallback, backgroundWorker, optimizationPreferences, dayOffOptimizationPreferenceProvider, runWeeklyRestSolver);
		}
	}
}