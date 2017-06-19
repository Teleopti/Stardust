using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ScheduleIslandExecutor : ScheduleExecutorOld
	{
		private readonly SchedulingCommandHandler _schedulingCommandHandler;

		public ScheduleIslandExecutor(Func<ISchedulerStateHolder> schedulerStateHolder, IRequiredScheduleHelper requiredScheduleOptimizerHelper, Func<IScheduleDayChangeCallback> scheduleDayChangeCallback, IScheduling teamBlockScheduling, ClassicScheduleCommand classicScheduleCommand, MatrixListFactory matrixListFactory, IWeeklyRestSolverCommand weeklyRestSolverCommand, CascadingResourceCalculationContextFactory resourceCalculationContextFactory, IUserTimeZone userTimeZone, IResourceCalculation resourceCalculation, SchedulingCommandHandler schedulingCommandHandler) 
			: base(schedulerStateHolder, requiredScheduleOptimizerHelper, scheduleDayChangeCallback, teamBlockScheduling, classicScheduleCommand, matrixListFactory, weeklyRestSolverCommand, resourceCalculationContextFactory, userTimeZone, resourceCalculation)
		{
			_schedulingCommandHandler = schedulingCommandHandler;
		}

		protected override void DoScheduling(ISchedulingCallback schedulingCallback, ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			bool runWeeklyRestSolver, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, SchedulingOptions schedulingOptions)
		{
			var command = new SchedulingCommand {Agents = selectedAgents, Period = selectedPeriod};
			_schedulingCommandHandler.Execute(command, schedulingOptions, schedulingCallback, backgroundWorker, dayOffOptimizationPreferenceProvider);
		}
	}
}