using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ScheduleExecutor
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Scheduling _teamBlockScheduling;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly RemoveShiftCategoryToBeInLegalState _removeShiftCategoryToBeInLegalState;
		private readonly RuleSetBagsOfGroupOfPeopleCanHaveShortBreak _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;

		public ScheduleExecutor(Func<ISchedulerStateHolder> schedulerStateHolder,
			Scheduling teamBlockScheduling,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			IResourceCalculation resourceCalculation,
			RemoveShiftCategoryToBeInLegalState removeShiftCategoryToBeInLegalState,
			RuleSetBagsOfGroupOfPeopleCanHaveShortBreak ruleSetBagsOfGroupOfPeopleCanHaveShortBreak)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_teamBlockScheduling = teamBlockScheduling;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_resourceCalculation = resourceCalculation;
			_removeShiftCategoryToBeInLegalState = removeShiftCategoryToBeInLegalState;
			_ruleSetBagsOfGroupOfPeopleCanHaveShortBreak = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
		}

		[TestLog]
		public virtual void Execute(ISchedulingCallback schedulingCallback, 
			SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker,
			IEnumerable<IPerson> selectedAgents, 
			DateOnlyPeriod selectedPeriod,
			IBlockPreferenceProvider blockPreferenceProvider)
		{
			
			var schedulerStateHolder = _schedulerStateHolder();
			schedulingOptions.DayOffTemplate = schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate;
			var lastCalculationState = schedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;
			var useShiftCategoryLimitations = schedulingOptions.UseShiftCategoryLimitations;
			schedulingOptions.UseShiftCategoryLimitations = false;

			using (_resourceCalculationContextFactory.Create(schedulerStateHolder.SchedulingResultState, true, selectedPeriod.Inflate(1)))
			{
				_resourceCalculation.ResourceCalculate(selectedPeriod.Inflate(1), new ResourceCalculationData(schedulerStateHolder.SchedulingResultState, schedulerStateHolder.ConsiderShortBreaks, false));
				schedulingOptions.OnlyShiftsWhenUnderstaffed = false;
				schedulingOptions.ConsiderShortBreaks = _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(selectedAgents, selectedPeriod);
				_teamBlockScheduling.Execute(schedulingCallback, schedulingOptions, backgroundWorker, selectedAgents, selectedPeriod, blockPreferenceProvider);
				if (!backgroundWorker.CancellationPending)
				{
					ExecuteWeeklyRestSolverCommand(useShiftCategoryLimitations, schedulingOptions, selectedAgents,selectedPeriod, backgroundWorker);
				}
			}
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}

		[TestLog]
		protected virtual void ExecuteWeeklyRestSolverCommand(bool useShiftCategoryLimitations, SchedulingOptions schedulingOptions,
															IEnumerable<IPerson> selectedPersons,
															DateOnlyPeriod selectedPeriod, 
															ISchedulingProgress backgroundWorker)
		{
			_removeShiftCategoryToBeInLegalState.Execute(useShiftCategoryLimitations, schedulingOptions, selectedPersons, selectedPeriod, backgroundWorker);
		}	
	}
}