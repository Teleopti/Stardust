using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ScheduleIslandExecutor : ScheduleExecutorOld
	{
		private readonly SchedulingCommandHandler _schedulingCommandHandler;

		public ScheduleIslandExecutor(Func<ISchedulerStateHolder> schedulerStateHolder, IScheduling teamBlockScheduling, ClassicScheduleCommand classicScheduleCommand, CascadingResourceCalculationContextFactory resourceCalculationContextFactory, IResourceCalculation resourceCalculation, SchedulingCommandHandler schedulingCommandHandler, ExecuteWeeklyRestSolver executeWeeklyRestSolver) 
			: base(schedulerStateHolder, teamBlockScheduling, classicScheduleCommand, resourceCalculationContextFactory, resourceCalculation, executeWeeklyRestSolver)
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