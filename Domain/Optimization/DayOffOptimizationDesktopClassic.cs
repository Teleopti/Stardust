using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998)]
	public class DayOffOptimizationDesktopClassic : IDayOffOptimizationDesktop
	{
		private readonly ClassicDaysOffOptimizationCommand _classicDaysOffOptimizationCommand;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly DaysOffBackToLegalState _daysOffBackToLegalState;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IResourceOptimizationHelperExtended _resouceOptimizationHelperExtended;
		private readonly WorkShiftBackToLegalStateServiceProFactory _workShiftBackToLegalStateServiceProFactory;
		private readonly ScheduleBlankSpots _scheduleBlankSpots;
		private readonly IResourceCalculationContextFactory _resourceCalculationContextFactory;

		public DayOffOptimizationDesktopClassic(ClassicDaysOffOptimizationCommand classicDaysOffOptimizationCommand,
								IMatrixListFactory matrixListFactory,
								IOptimizerHelperHelper optimizerHelperHelper,
								Func<ISchedulerStateHolder> schedulerStateHolder,
								DaysOffBackToLegalState daysOffBackToLegalState,
								Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
								IScheduleDayEquator scheduleDayEquator,
								IResourceOptimizationHelperExtended resouceOptimizationHelperExtended,
								WorkShiftBackToLegalStateServiceProFactory workShiftBackToLegalStateServiceProFactory,
								ScheduleBlankSpots scheduleBlankSpots,
								IResourceCalculationContextFactory resourceCalculationContextFactory)
		{
			_classicDaysOffOptimizationCommand = classicDaysOffOptimizationCommand;
			_matrixListFactory = matrixListFactory;
			_optimizerHelperHelper = optimizerHelperHelper;
			_schedulerStateHolder = schedulerStateHolder;
			_daysOffBackToLegalState = daysOffBackToLegalState;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_scheduleDayEquator = scheduleDayEquator;
			_resouceOptimizationHelperExtended = resouceOptimizationHelperExtended;
			_workShiftBackToLegalStateServiceProFactory = workShiftBackToLegalStateServiceProFactory;
			_scheduleBlankSpots = scheduleBlankSpots;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
		}

		public void Execute(DateOnlyPeriod selectedPeriod, IEnumerable<IScheduleDay> selectedDays,
			ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, GroupPageLight groupPageLight,
			Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder, Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			var stateHolder = _schedulerStateHolder();
			var matrixListForDayOffOptimization = _matrixListFactory.CreateMatrixListForSelection(selectedDays);
			var matrixContainerList = createMatrixContainerList(matrixListForDayOffOptimization);
			var matrixList = matrixContainerList.Select(container => container.ScheduleMatrix).ToList();

			_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixList, optimizationPreferences, selectedPeriod);

			resourceOptimizerPersonOptimized(this, new ResourceOptimizerProgressEventArgs(0, 0, Resources.DaysOffBackToLegalState + Resources.ThreeDots));

			// to make sure we are in legal state before we can do day off optimization
			var displayList = stateHolder.CommonStateHolder.ActiveDayOffs.ToList();
			displayList.Sort(new DayOffTemplateSorter());
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			_daysOffBackToLegalState.Execute(matrixContainerList, backgroundWorker, displayList[0], schedulingOptions, dayOffOptimizationPreferenceProvider, optimizationPreferences, workShiftFinderResultHolder, resourceOptimizerPersonOptimized);

			var workShiftBackToLegalStateService = _workShiftBackToLegalStateServiceProFactory.Create();

			var rollbackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, _scheduleDayChangeCallback(), new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			foreach (var matrixOriginalStateContainer in matrixContainerList)
			{
				rollbackService.ClearModificationCollection();
				workShiftBackToLegalStateService.Execute(matrixOriginalStateContainer.ScheduleMatrix, schedulingOptions, rollbackService);
			}

			resourceOptimizerPersonOptimized(this, new ResourceOptimizerProgressEventArgs(0, 0, Resources.Rescheduling + Resources.ThreeDots));
			// schedule those are the white spots after back to legal state
			_scheduleBlankSpots.Execute(matrixContainerList, optimizationPreferences);

			var validMatrixContainerList = new List<IScheduleMatrixOriginalStateContainer>();
			rollbackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, _scheduleDayChangeCallback(), new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
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

			using (_resourceCalculationContextFactory.Create(stateHolder.Schedules, stateHolder.SchedulingResultState.Skills))
			{
				ResourceCalculationContext.Fetch().PrimarySkillMode = true;
				_resouceOptimizationHelperExtended.ResourceCalculateAllDays(backgroundWorker, false);
				optimize(validMatrixContainerList, selectedPeriod, backgroundWorker, optimizationPreferences, dayOffOptimizationPreferenceProvider);
			}

			foreach (var matrixContainer in validMatrixContainerList)
			{
				if (!matrixContainer.IsFullyScheduled())
				{
					rollbackMatrixChanges(matrixContainer, rollbackService, resourceOptimizerPersonOptimized);
				}
			}
		}

		private void optimize(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization, DateOnlyPeriod selectedPeriod,
	ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences,
	IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_classicDaysOffOptimizationCommand.Execute(matrixOriginalStateContainerListForDayOffOptimization, selectedPeriod,
				optimizationPreferences, backgroundWorker, dayOffOptimizationPreferenceProvider);
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
	}
}