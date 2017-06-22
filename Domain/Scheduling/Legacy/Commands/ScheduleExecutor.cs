using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public interface IScheduleExecutor
	{
		void Execute(ISchedulingCallback schedulingCallback, 
			SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker,
			IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences, bool runWeeklyRestSolver,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}

	public class ScheduleExecutor : ScheduleExecutorOld
	{
		private readonly RuleSetBagsOfGroupOfPeopleCanHaveShortBreak _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;

		public ScheduleExecutor(Func<ISchedulerStateHolder> schedulerStateHolder, IScheduling teamBlockScheduling, ClassicScheduleCommand classicScheduleCommand, CascadingResourceCalculationContextFactory resourceCalculationContextFactory, IResourceCalculation resourceCalculation,
			RuleSetBagsOfGroupOfPeopleCanHaveShortBreak ruleSetBagsOfGroupOfPeopleCanHaveShortBreak, RemoveShiftCategoryToBeInLegalState removeShiftCategoryToBeInLegalState) 
			: base(schedulerStateHolder, teamBlockScheduling, classicScheduleCommand, resourceCalculationContextFactory, resourceCalculation, removeShiftCategoryToBeInLegalState)
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

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public class ScheduleExecutorOld : IScheduleExecutor
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		[RemoveMeWithToggle("make private", Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
		protected readonly IScheduling TeamBlockScheduling;
		[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
		private readonly ClassicScheduleCommand _classicScheduleCommand;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly RemoveShiftCategoryToBeInLegalState _removeShiftCategoryToBeInLegalState;

		public ScheduleExecutorOld(Func<ISchedulerStateHolder> schedulerStateHolder,
			IScheduling teamBlockScheduling,
			ClassicScheduleCommand classicScheduleCommand,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			IResourceCalculation resourceCalculation,
			RemoveShiftCategoryToBeInLegalState removeShiftCategoryToBeInLegalState)
		{
			_schedulerStateHolder = schedulerStateHolder;
			TeamBlockScheduling = teamBlockScheduling;
			_classicScheduleCommand = classicScheduleCommand;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_resourceCalculation = resourceCalculation;
			_removeShiftCategoryToBeInLegalState = removeShiftCategoryToBeInLegalState;
		}

		[TestLog]
		public virtual void Execute(ISchedulingCallback schedulingCallback, 
			SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker,
			IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences, bool runWeeklyRestSolver,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			schedulingOptions.DayOffTemplate = schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate;
			var lastCalculationState = schedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;
			//set to false for first scheduling and then use it for RemoveShiftCategoryBackToLegalState
			var useShiftCategoryLimitations = schedulingOptions.UseShiftCategoryLimitations;
			schedulingOptions.UseShiftCategoryLimitations = false;

			using (_resourceCalculationContextFactory.Create(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.Skills, true, selectedPeriod.Inflate(1)))
			{
				_resourceCalculation.ResourceCalculate(selectedPeriod.Inflate(1), new ResourceCalculationData(schedulerStateHolder.SchedulingResultState, schedulerStateHolder.ConsiderShortBreaks, false));
				schedulingOptions.OnlyShiftsWhenUnderstaffed = false;
				DoScheduling(schedulingCallback, backgroundWorker, selectedAgents, selectedPeriod, runWeeklyRestSolver, dayOffOptimizationPreferenceProvider, schedulingOptions);

				if (!backgroundWorker.CancellationPending)
				{
					ExecuteWeeklyRestSolverCommand(useShiftCategoryLimitations, schedulingOptions, optimizationPreferences, selectedAgents,
							selectedPeriod, backgroundWorker, dayOffOptimizationPreferenceProvider);
				}
			}

			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}

		protected virtual void DoScheduling(ISchedulingCallback schedulingCallback, ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			bool runWeeklyRestSolver, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			SchedulingOptions schedulingOptions)
		{
			if (schedulingOptions.UseBlock || schedulingOptions.UseTeam)
			{
				TeamBlockScheduling.Execute(new NoSchedulingCallback(), schedulingOptions, backgroundWorker, selectedAgents, selectedPeriod, dayOffOptimizationPreferenceProvider);
			}
			else
			{
				_classicScheduleCommand.Execute(schedulingOptions, backgroundWorker, selectedAgents, selectedPeriod, dayOffOptimizationPreferenceProvider, runWeeklyRestSolver);
			}
		}


		[TestLog]
		protected virtual void ExecuteWeeklyRestSolverCommand(bool useShiftCategoryLimitations, SchedulingOptions schedulingOptions,
															IOptimizationPreferences optimizationPreferences, IEnumerable<IPerson> selectedPersons,
															DateOnlyPeriod selectedPeriod, 
															ISchedulingProgress backgroundWorker, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_removeShiftCategoryToBeInLegalState.Execute(useShiftCategoryLimitations, schedulingOptions, optimizationPreferences, selectedPersons, selectedPeriod, backgroundWorker, dayOffOptimizationPreferenceProvider);
		}	
	}
}