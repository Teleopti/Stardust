using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ScheduleIslandExecutor : ScheduleExecutorOld
	{
		private readonly RuleSetBagsOfGroupOfPeopleCanHaveShortBreak _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;

		public ScheduleIslandExecutor(Func<ISchedulerStateHolder> schedulerStateHolder, IRequiredScheduleHelper requiredScheduleOptimizerHelper, Func<IScheduleDayChangeCallback> scheduleDayChangeCallback, IScheduling teamBlockScheduling, ClassicScheduleCommand classicScheduleCommand, MatrixListFactory matrixListFactory, IWeeklyRestSolverCommand weeklyRestSolverCommand, CascadingResourceCalculationContextFactory resourceCalculationContextFactory, IUserTimeZone userTimeZone, IResourceCalculation resourceCalculation, RuleSetBagsOfGroupOfPeopleCanHaveShortBreak ruleSetBagsOfGroupOfPeopleCanHaveShortBreak) 
			: base(schedulerStateHolder, requiredScheduleOptimizerHelper, scheduleDayChangeCallback, teamBlockScheduling, classicScheduleCommand, matrixListFactory, weeklyRestSolverCommand, resourceCalculationContextFactory, userTimeZone, resourceCalculation)
		{
			_ruleSetBagsOfGroupOfPeopleCanHaveShortBreak = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
		}

		protected override void DoScheduling(ISchedulingCallback schedulingCallback, ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			bool runWeeklyRestSolver, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, SchedulingOptions schedulingOptions)
		{
			schedulingOptions.ConsiderShortBreaks = _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(selectedAgents, selectedPeriod);
			TeamBlockScheduling.Execute(schedulingCallback, schedulingOptions, backgroundWorker, selectedAgents, selectedPeriod, dayOffOptimizationPreferenceProvider);
		}
	}
}