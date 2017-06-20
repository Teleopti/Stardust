using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_SchedulingIslands_44757)]
	public interface ISchedulingCommandHandler
	{
		//TODO - should accept SchedulingCommand
		void Execute(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences, bool runWeeklyRestSolver,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_SchedulingIslands_44757)]
	public class SchedulingCommandHandlerPassThrough : ISchedulingCommandHandler
	{
		private readonly IScheduleExecutor _scheduleExecutor;

		public SchedulingCommandHandlerPassThrough(IScheduleExecutor scheduleExecutor)
		{
			_scheduleExecutor = scheduleExecutor;
		}

		public void Execute(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences, bool runWeeklyRestSolver,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_scheduleExecutor.Execute(schedulingCallback, schedulingOptions, backgroundWorker, selectedAgents, selectedPeriod, optimizationPreferences, runWeeklyRestSolver, dayOffOptimizationPreferenceProvider);
		}
	}
}