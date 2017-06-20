using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingCommandHandler : ISchedulingCommandHandler
	{
		private readonly SchedulingEventHandler _schedulingEventHandler;

		public SchedulingCommandHandler(SchedulingEventHandler schedulingEventHandler)
		{
			_schedulingEventHandler = schedulingEventHandler;
		}

		public void Execute(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences, bool runWeeklyRestSolver,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var @event = new SchedulingWasOrdered
			{
				AgentsToOptimize = selectedAgents.Select(x => x.Id.Value),
				StartDate = selectedPeriod.StartDate,
				EndDate = selectedPeriod.EndDate
			};
			_schedulingEventHandler.HandleEvent(@event, schedulingOptions, schedulingCallback, backgroundWorker, optimizationPreferences, dayOffOptimizationPreferenceProvider, runWeeklyRestSolver);
		}
	}
}