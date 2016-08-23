using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
		private readonly OptimizerHelperHelper _optimizerHelper;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private readonly IDailySkillForecastAndScheduledValueCalculator _dailySkillForecastAndScheduledValueCalculator;
		private readonly SchedulingStateHolderAllSkillExtractor _schedulingStateHolderAllSkillExtractor;
		private readonly IWorkShiftLegalStateDayIndexCalculator _workShiftLegalStateDayIndexCalculator;
		private readonly IDeleteSchedulePartService _deleteSchedulePartService;
		private readonly IScheduleService _scheduleService;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IResourceOptimizationHelperExtended _resouceOptimizationHelperExtended;

		protected DayOffOptimizationDesktop(IMatrixListFactory matrixListFactory, 
								IOptimizerHelperHelper optimizerHelperHelper, 
								Func<ISchedulerStateHolder> schedulerStateHolder,
								DaysOffBackToLegalState daysOffBackToLegalState,
								OptimizerHelperHelper optimizerHelper,
								IResourceOptimizationHelper resourceOptimizationHelper,
								Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
								IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
								IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator,
								SchedulingStateHolderAllSkillExtractor schedulingStateHolderAllSkillExtractor,
								IWorkShiftLegalStateDayIndexCalculator workShiftLegalStateDayIndexCalculator,
								IDeleteSchedulePartService deleteSchedulePartService,
								IScheduleService scheduleService,
								IEffectiveRestrictionCreator effectiveRestrictionCreator,
								IScheduleDayEquator scheduleDayEquator,
								IResourceOptimizationHelperExtended resouceOptimizationHelperExtended)
		{
			_matrixListFactory = matrixListFactory;
			_optimizerHelperHelper = optimizerHelperHelper;
			_schedulerStateHolder = schedulerStateHolder;
			_daysOffBackToLegalState = daysOffBackToLegalState;
			_optimizerHelper = optimizerHelper;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
			_dailySkillForecastAndScheduledValueCalculator = dailySkillForecastAndScheduledValueCalculator;
			_schedulingStateHolderAllSkillExtractor = schedulingStateHolderAllSkillExtractor;
			_workShiftLegalStateDayIndexCalculator = workShiftLegalStateDayIndexCalculator;
			_deleteSchedulePartService = deleteSchedulePartService;
			_scheduleService = scheduleService;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_scheduleDayEquator = scheduleDayEquator;
			_resouceOptimizationHelperExtended = resouceOptimizationHelperExtended;
		}


		public void Execute(DateOnlyPeriod selectedPeriod, IEnumerable<IScheduleDay> selectedDays,
			ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, 
			Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder, Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{

			IList<IScheduleMatrixPro> matrixListForDayOffOptimization = _matrixListFactory.CreateMatrixListForSelection(selectedDays);
			IList<IScheduleMatrixOriginalStateContainer> matrixContainerList = createMatrixContainerList(matrixListForDayOffOptimization);

			IList<IScheduleMatrixPro> matrixList = matrixContainerList.Select(container => container.ScheduleMatrix).ToList();

			_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixList, optimizationPreferences, selectedPeriod);

			var e = new ResourceOptimizerProgressEventArgs(0, 0, Resources.DaysOffBackToLegalState + Resources.ThreeDots);
			resourceOptimizerPersonOptimized(this, e);

			// to make sure we are in legal state before we can do day off optimization
			var displayList = _schedulerStateHolder().CommonStateHolder.ActiveDayOffs.ToList();
			displayList.Sort(new DayOffTemplateSorter());
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			_daysOffBackToLegalState.Execute(matrixContainerList, backgroundWorker, displayList[0], schedulingOptions, dayOffOptimizationPreferenceProvider, optimizationPreferences, workShiftFinderResultHolder, resourceOptimizerPersonOptimized);

			//TODO: move dep to this service
			var workShiftBackToLegalStateService =
				_optimizerHelper.CreateWorkShiftBackToLegalStateServicePro(_workShiftMinMaxCalculator, _dailySkillForecastAndScheduledValueCalculator, _schedulingStateHolderAllSkillExtractor, _workShiftLegalStateDayIndexCalculator, _deleteSchedulePartService);

			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState, _scheduleDayChangeCallback(),
					new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			foreach (var matrixOriginalStateContainer in matrixContainerList)
			{
				rollbackService.ClearModificationCollection();
				workShiftBackToLegalStateService.Execute(matrixOriginalStateContainer.ScheduleMatrix, schedulingOptions,
					rollbackService);
			}

			e = new ResourceOptimizerProgressEventArgs(0, 0, Resources.Rescheduling + Resources.ThreeDots);
			resourceOptimizerPersonOptimized(this, e);


			// schedule those are the white spots after back to legal state
			_optimizerHelper.ScheduleBlankSpots(matrixContainerList, _scheduleService, rollbackService, _schedulerStateHolder().SchedulingResultState,
				_effectiveRestrictionCreator,
				optimizationPreferences,
				_resourceOptimizationHelper);

			IList<IScheduleMatrixOriginalStateContainer> validMatrixContainerList = new List<IScheduleMatrixOriginalStateContainer>();
			rollbackService = new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState, _scheduleDayChangeCallback(),
				new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
			foreach (IScheduleMatrixOriginalStateContainer matrixContainer in matrixContainerList)
			{
				bool isFullyScheduled = matrixContainer.IsFullyScheduled();
				if (!isFullyScheduled)
				{
					rollbackMatrixChanges(matrixContainer, rollbackService, resourceOptimizerPersonOptimized);
					continue;
				}
				validMatrixContainerList.Add(matrixContainer);
			}
			_resouceOptimizationHelperExtended.ResourceCalculateAllDays(backgroundWorker, false);

			Optimize(validMatrixContainerList, selectedPeriod, backgroundWorker, optimizationPreferences, dayOffOptimizationPreferenceProvider);
			after(validMatrixContainerList, rollbackService, resourceOptimizerPersonOptimized);
		}

		private void after(IEnumerable<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization, ISchedulePartModifyAndRollbackService rollbackService, Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			foreach (var matrixContainer in matrixOriginalStateContainerListForDayOffOptimization)
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