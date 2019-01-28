using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization.ClassicLegacy;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourcePlanner;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class ScheduleAllRemovedDaysOrRollback
	{
		private readonly ScheduleBlankSpots _scheduleBlankSpots;

		public ScheduleAllRemovedDaysOrRollback(ScheduleBlankSpots scheduleBlankSpots)
		{
			_scheduleBlankSpots = scheduleBlankSpots;
		}
		public void Execute(ScheduleMatrixOriginalStateContainer matrixOriginalStateContainer, IOptimizationPreferences optimizationPreferences, IEnumerable<DateOnly> removedDays, SchedulePartModifyAndRollbackService rollbackService)
		{
			if (removedDays.IsEmpty()) return;

			_scheduleBlankSpots.Execute(new[] { matrixOriginalStateContainer }, optimizationPreferences);

			if (removedDays.Any(removedDay => !matrixOriginalStateContainer.ScheduleMatrix.GetScheduleDayByKey(removedDay).DaySchedulePart().IsScheduled()))
			{
				rollbackService.Rollback();
			}
		}
	}

	public class DayOffOptimization
	{
		private readonly TeamBlockDayOffOptimizer _teamBlockDayOffOptimizer;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly WorkShiftBackToLegalStateServiceProFactory _workShiftBackToLegalStateServiceProFactory;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly DaysOffBackToLegalState _daysOffBackToLegalState;
		private readonly ScheduleBlankSpots _scheduleBlankSpots;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;
		private readonly IBlockPreferenceProviderForPlanningPeriod _blockPreferenceProviderForPlanningPeriod;
		private readonly IDayOffOptimizationPreferenceProviderForPlanningPeriod _dayOffOptimizationPreferenceProviderForPlanningPeriod;
		private readonly ScheduleAllRemovedDaysOrRollback _scheduleAllRemovedDaysOrRollback;
		private readonly IUserTimeZone _userTimeZone;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly IPlanningGroupGlobalSettingSetter _planningGroupGlobalSettingSetter;
		

		public DayOffOptimization(TeamBlockDayOffOptimizer teamBlockDayOffOptimizer,
			WeeklyRestSolverExecuter weeklyRestSolverExecuter,
			IResourceCalculation resourceCalculation,
			IUserTimeZone userTimeZone,
			TeamInfoFactoryFactory teamInfoFactoryFactory,
			MatrixListFactory matrixListFactory,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IScheduleDayEquator scheduleDayEquator,
			WorkShiftBackToLegalStateServiceProFactory workShiftBackToLegalStateServiceProFactory,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			DaysOffBackToLegalState daysOffBackToLegalState,
			ScheduleBlankSpots scheduleBlankSpots,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			IOptimizationPreferencesProvider optimizationPreferencesProvider,
			IBlockPreferenceProviderForPlanningPeriod blockPreferenceProviderForPlanningPeriod,
			IDayOffOptimizationPreferenceProviderForPlanningPeriod dayOffOptimizationPreferenceProviderForPlanningPeriod,
			ScheduleAllRemovedDaysOrRollback scheduleAllRemovedDaysOrRollback,
			IPlanningGroupGlobalSettingSetter planningGroupGlobalSettingSetter)
		{
			_teamBlockDayOffOptimizer = teamBlockDayOffOptimizer;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_resourceCalculation = resourceCalculation;
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayEquator = scheduleDayEquator;
			_workShiftBackToLegalStateServiceProFactory = workShiftBackToLegalStateServiceProFactory;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_daysOffBackToLegalState = daysOffBackToLegalState;
			_scheduleBlankSpots = scheduleBlankSpots;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
			_blockPreferenceProviderForPlanningPeriod = blockPreferenceProviderForPlanningPeriod;
			_dayOffOptimizationPreferenceProviderForPlanningPeriod = dayOffOptimizationPreferenceProviderForPlanningPeriod;
			_scheduleAllRemovedDaysOrRollback = scheduleAllRemovedDaysOrRollback;
			_planningGroupGlobalSettingSetter = planningGroupGlobalSettingSetter;
			_userTimeZone = userTimeZone;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
			_matrixListFactory = matrixListFactory;
		}

		public void Execute(DateOnlyPeriod selectedPeriod,
			IEnumerable<IPerson> selectedAgents,
			bool runWeeklyRestSolver, // desktop client runs this explicitly afterwards so sending in false here
			AllSettingsForPlanningGroup allSettingsForPlanningGroup)
		{
			var optimizationPreferences = _optimizationPreferencesProvider.Fetch();
			_planningGroupGlobalSettingSetter.Execute(allSettingsForPlanningGroup, optimizationPreferences);
			var blockPreferenceProvider = _blockPreferenceProviderForPlanningPeriod.Fetch(allSettingsForPlanningGroup);
			var dayOffOptimizationPreferenceProvider = _dayOffOptimizationPreferenceProviderForPlanningPeriod.Fetch(allSettingsForPlanningGroup);
			var stateHolder = _schedulerStateHolder();
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			var resourceCalcDelayer = new ResourceCalculateDelayer(_resourceCalculation, schedulingOptions.ConsiderShortBreaks, stateHolder.SchedulingResultState, _userTimeZone);

			using (_resourceCalculationContextFactory.Create(stateHolder.SchedulingResultState, true, selectedPeriod.Inflate(1)))
			{
				IEnumerable<IScheduleMatrixPro> matrixList;
				if (optimizationPreferences.Extra.IsClassic())
				{
					//TO SIMULATE OLD CLASSIC BEHAVIOR (diff behavior between classic and teamblock)
					//TODO: When this code is removed/rewritten for teamblock, a lot of these types and its children can be deleted (only used here)
					var scheduleDays = _schedulerStateHolder().Schedules.SchedulesForPeriod(selectedPeriod, selectedAgents.ToArray());
					var nonFullyScheduledAgents = scheduleDays.Where(x => !x.IsScheduled()).Select(x => x.Person);
					var filteredAgents = selectedAgents.Except(nonFullyScheduledAgents).ToArray();
					matrixList = _matrixListFactory.CreateMatrixListForSelection(stateHolder.Schedules, filteredAgents, selectedPeriod);
					var matrixListOriginalStateContainer = matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator)).ToArray();
					_daysOffBackToLegalState.Execute(matrixListOriginalStateContainer,
						schedulingOptions,
						dayOffOptimizationPreferenceProvider,
						optimizationPreferences);
					var workShiftBackToLegalStateService = _workShiftBackToLegalStateServiceProFactory.Create();
					foreach (var matrixOriginalStateContainer in matrixListOriginalStateContainer)
					{
						var rollbackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, _scheduleDayChangeCallback(), new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
						var removedDays = workShiftBackToLegalStateService.Execute(matrixOriginalStateContainer.ScheduleMatrix, schedulingOptions, rollbackService );
						_scheduleAllRemovedDaysOrRollback.Execute(matrixOriginalStateContainer, optimizationPreferences, removedDays, rollbackService);
					}
					_scheduleBlankSpots.Execute(matrixListOriginalStateContainer, optimizationPreferences);
					//////////////////
				}
				else
				{
					matrixList = _matrixListFactory.CreateMatrixListForSelection(stateHolder.Schedules, selectedAgents, selectedPeriod);
				}
				var selectedPersons = matrixList.Select(x => x.Person).Distinct().ToList();
				
				_resourceCalculation.ResourceCalculate(selectedPeriod.Inflate(1), new ResourceCalculationData(stateHolder.SchedulingResultState, false, false));
				var teamInfoFactory = _teamInfoFactoryFactory.Create(stateHolder.ChoosenAgents, stateHolder.Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);
			
				_teamBlockDayOffOptimizer.OptimizeDaysOff(matrixList,
					selectedPeriod,
					selectedPersons,
					optimizationPreferences,
					schedulingOptions,
					resourceCalcDelayer,
					dayOffOptimizationPreferenceProvider,
					blockPreferenceProvider,
					teamInfoFactory,
					new NoSchedulingProgress());
				
				if (runWeeklyRestSolver)
				{
					_weeklyRestSolverExecuter.Resolve(
						optimizationPreferences,
						selectedPeriod,
						selectedPersons,
						dayOffOptimizationPreferenceProvider);
				}	
			}
		}
	}
}