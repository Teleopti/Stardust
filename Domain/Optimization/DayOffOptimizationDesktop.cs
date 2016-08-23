using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public abstract class DayOffOptimizationDesktop : IDayOffOptimizationDesktop
	{
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly DaysOffBackToLegalState _daysOffBackToLegalState;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IResourceOptimizationHelperExtended _resouceOptimizationHelperExtended;
		private readonly WorkShiftBackToLegalStateServiceProFactory _workShiftBackToLegalStateServiceProFactory;
		private readonly ScheduleBlankSpots _scheduleBlankSpots;

		protected DayOffOptimizationDesktop(IMatrixListFactory matrixListFactory, 
								IOptimizerHelperHelper optimizerHelperHelper, 
								Func<ISchedulerStateHolder> schedulerStateHolder,
								DaysOffBackToLegalState daysOffBackToLegalState,
								Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
								IScheduleDayEquator scheduleDayEquator,
								IResourceOptimizationHelperExtended resouceOptimizationHelperExtended,
								WorkShiftBackToLegalStateServiceProFactory workShiftBackToLegalStateServiceProFactory,
								ScheduleBlankSpots scheduleBlankSpots)
		{
			_matrixListFactory = matrixListFactory;
			_optimizerHelperHelper = optimizerHelperHelper;
			_schedulerStateHolder = schedulerStateHolder;
			_daysOffBackToLegalState = daysOffBackToLegalState;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_scheduleDayEquator = scheduleDayEquator;
			_resouceOptimizationHelperExtended = resouceOptimizationHelperExtended;
			_workShiftBackToLegalStateServiceProFactory = workShiftBackToLegalStateServiceProFactory;
			_scheduleBlankSpots = scheduleBlankSpots;
		}


		public void Execute(DateOnlyPeriod selectedPeriod, IEnumerable<IScheduleDay> selectedDays,
			ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, 
			Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder, Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			var matrixListForDayOffOptimization = _matrixListFactory.CreateMatrixListForSelection(selectedDays);
			var matrixContainerList = createMatrixContainerList(matrixListForDayOffOptimization);
			var matrixList = matrixContainerList.Select(container => container.ScheduleMatrix).ToList();

			_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixList, optimizationPreferences, selectedPeriod);

			resourceOptimizerPersonOptimized(this, new ResourceOptimizerProgressEventArgs(0, 0, Resources.DaysOffBackToLegalState + Resources.ThreeDots));

			// to make sure we are in legal state before we can do day off optimization
			var displayList = _schedulerStateHolder().CommonStateHolder.ActiveDayOffs.ToList();
			displayList.Sort(new DayOffTemplateSorter());
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			_daysOffBackToLegalState.Execute(matrixContainerList, backgroundWorker, displayList[0], schedulingOptions, dayOffOptimizationPreferenceProvider, optimizationPreferences, workShiftFinderResultHolder, resourceOptimizerPersonOptimized);

			var workShiftBackToLegalStateService = _workShiftBackToLegalStateServiceProFactory.Create();

			var rollbackService = new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState, _scheduleDayChangeCallback(), new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			foreach (var matrixOriginalStateContainer in matrixContainerList)
			{
				rollbackService.ClearModificationCollection();
				workShiftBackToLegalStateService.Execute(matrixOriginalStateContainer.ScheduleMatrix, schedulingOptions, rollbackService);
			}

			resourceOptimizerPersonOptimized(this, new ResourceOptimizerProgressEventArgs(0, 0, Resources.Rescheduling + Resources.ThreeDots));
			// schedule those are the white spots after back to legal state
			_scheduleBlankSpots.Execute(matrixContainerList, optimizationPreferences);

			var validMatrixContainerList = new List<IScheduleMatrixOriginalStateContainer>();
			rollbackService = new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState, _scheduleDayChangeCallback(), new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
			foreach (IScheduleMatrixOriginalStateContainer matrixContainer in matrixContainerList)
			{
				var isFullyScheduled = matrixContainer.IsFullyScheduled();
				if (!isFullyScheduled)
				{
					rollbackMatrixChanges(matrixContainer, rollbackService, resourceOptimizerPersonOptimized);
					continue;
				}
				validMatrixContainerList.Add(matrixContainer);
			}
			_resouceOptimizationHelperExtended.ResourceCalculateAllDays(backgroundWorker, false);

			Optimize(validMatrixContainerList, selectedPeriod, backgroundWorker, optimizationPreferences, dayOffOptimizationPreferenceProvider);

			foreach (var matrixContainer in validMatrixContainerList)
			{
				if (!matrixContainer.IsFullyScheduled())
				{
					rollbackMatrixChanges(matrixContainer, rollbackService, resourceOptimizerPersonOptimized);
				}
			}
		}

		private void rollbackMatrixChanges(IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer, ISchedulePartModifyAndRollbackService rollbackService, Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			var e = new ResourceOptimizerProgressEventArgs(0, 0, Resources.RollingBackSchedulesFor + " " + matrixOriginalStateContainer.ScheduleMatrix.Person.Name);
			resourceOptimizerPersonOptimized(this, e);
			if (e.Cancel) return;

			rollbackService.ClearModificationCollection();
			foreach (var scheduleDayPro in matrixOriginalStateContainer.ScheduleMatrix.EffectivePeriodDays)
			{
				var originalPart = matrixOriginalStateContainer.OldPeriodDaysState[scheduleDayPro.Day];
				rollbackService.Modify(originalPart);
			}
		}

		private IList<IScheduleMatrixOriginalStateContainer> createMatrixContainerList(IEnumerable<IScheduleMatrixPro> matrixList)
		{
			var result = matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator))
					.Cast<IScheduleMatrixOriginalStateContainer>().ToList();
			return result;
		}

		protected abstract void Optimize(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization, DateOnlyPeriod selectedPeriod, ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}
}