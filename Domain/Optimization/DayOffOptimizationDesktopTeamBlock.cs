using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.ClassicLegacy;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationDesktopTeamBlock
	{
		private readonly DayOffOptimization _dayOffOptimization;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly DaysOffBackToLegalState _daysOffBackToLegalState;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly ScheduleBlankSpots _scheduleBlankSpots;
		private readonly WorkShiftBackToLegalStateServiceProFactory _workShiftBackToLegalStateServiceProFactory;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;

		public DayOffOptimizationDesktopTeamBlock(DayOffOptimization dayOffOptimization,
								MatrixListFactory matrixListFactory,
								Func<ISchedulerStateHolder> schedulerStateHolder,
								CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
								DaysOffBackToLegalState daysOffBackToLegalState,
								IScheduleDayEquator scheduleDayEquator,
								ScheduleBlankSpots scheduleBlankSpots,
								WorkShiftBackToLegalStateServiceProFactory workShiftBackToLegalStateServiceProFactory,
								Func<IScheduleDayChangeCallback> scheduleDayChangeCallback)
		{
			_dayOffOptimization = dayOffOptimization;
			_matrixListFactory = matrixListFactory;
			_schedulerStateHolder = schedulerStateHolder;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_daysOffBackToLegalState = daysOffBackToLegalState;
			_scheduleDayEquator = scheduleDayEquator;
			_scheduleBlankSpots = scheduleBlankSpots;
			_workShiftBackToLegalStateServiceProFactory = workShiftBackToLegalStateServiceProFactory;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public void Execute(DateOnlyPeriod selectedPeriod, 
			IEnumerable<IPerson> selectedAgents, 
			ISchedulingProgress backgroundWorker,
			IOptimizationPreferences optimizationPreferences, 
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			var stateHolder = _schedulerStateHolder();
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			var blockPreferenceProvider = new FixedBlockPreferenceProvider(optimizationPreferences.Extra);

			using (_resourceCalculationContextFactory.Create(stateHolder.SchedulingResultState, true, selectedPeriod.Inflate(1)))
			{
				IEnumerable<IScheduleMatrixPro> matrixList;
				if (optimizationPreferences.Extra.IsClassic())
				{
					//TO SIMULATE OLD CLASSIC BEHAVIOR (diff behavior between classic and teamblock)
					var scheduleDays = _schedulerStateHolder().Schedules.SchedulesForPeriod(selectedPeriod, selectedAgents.ToArray());
					var nonFullyScheduledAgents = scheduleDays.Where(x => !x.IsScheduled()).Select(x => x.Person);
					var filteredAgents = selectedAgents.Except(nonFullyScheduledAgents).ToArray();
					matrixList = _matrixListFactory.CreateMatrixListForSelection(stateHolder.Schedules, filteredAgents, selectedPeriod);
					var matrixListOriginalStateContainer = matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator)).ToArray();
					_daysOffBackToLegalState.Execute(matrixListOriginalStateContainer,
													schedulingOptions,
													dayOffOptimizationPreferenceProvider,
													optimizationPreferences,
													resourceOptimizerPersonOptimized);
					var workShiftBackToLegalStateService = _workShiftBackToLegalStateServiceProFactory.Create();
					foreach (var matrixOriginalStateContainer in matrixListOriginalStateContainer)
					{
						workShiftBackToLegalStateService.Execute(matrixOriginalStateContainer.ScheduleMatrix, schedulingOptions, new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, _scheduleDayChangeCallback(), new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling)));
					}
					_scheduleBlankSpots.Execute(matrixListOriginalStateContainer, optimizationPreferences);
					//////////////////
				}
				else
				{
					matrixList = _matrixListFactory.CreateMatrixListForSelection(stateHolder.Schedules, selectedAgents, selectedPeriod);
				}
				var selectedPersons = matrixList.Select(x => x.Person).Distinct().ToList();
				
				_dayOffOptimization.Execute(matrixList, selectedPeriod, selectedPersons, optimizationPreferences, schedulingOptions, 
					dayOffOptimizationPreferenceProvider, blockPreferenceProvider, backgroundWorker, false);
			}
		}
	}
}