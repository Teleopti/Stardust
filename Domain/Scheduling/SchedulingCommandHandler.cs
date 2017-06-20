using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingCommandHandler
	{
		private readonly SchedulingEventHandler _schedulingEventHandler;

		public SchedulingCommandHandler(SchedulingEventHandler schedulingEventHandler)
		{
			_schedulingEventHandler = schedulingEventHandler;
		}

		public void Execute(SchedulingCommand schedulingCommand, ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var @event = new SchedulingWasOrdered
			{
				AgentsToSchedule = schedulingCommand.AgentsToSchedule.Select(x => x.Id.Value),
				StartDate = schedulingCommand.Period.StartDate,
				EndDate = schedulingCommand.Period.EndDate,
				RunWeeklyRestSolver = schedulingCommand.RunWeeklyRestSolver,
				CommandId = schedulingCommand.CommandId
			};
			_schedulingEventHandler.HandleEvent(@event, backgroundWorker, optimizationPreferences, dayOffOptimizationPreferenceProvider);
		}
	}
}