using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class BackToLegalStateExecuter
	{
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly PeriodExtractorFromScheduleParts _periodExtractor;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly ScheduleMatrixOriginalStateContainerCreator _scheduleMatrixOriginalStateContainerCreator;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly WorkShiftBackToLegalStateServiceProFactory _workShiftBackToLegalStateServiceProFactory;
		private readonly IRequiredScheduleHelper _requiredScheduleHelper;
		private readonly ScheduleOptimizerHelper _scheduleOptimizerHelper;
		private readonly DoFullResourceOptimizationOneTime _doFullResourceOptimizationOneTime;

		public BackToLegalStateExecuter(IScheduleDayChangeCallback scheduleDayChangeCallback,
			IMatrixListFactory matrixListFactory,
			PeriodExtractorFromScheduleParts periodExtractor,
			Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended,
			ScheduleMatrixOriginalStateContainerCreator scheduleMatrixOriginalStateContainerCreator,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			WorkShiftBackToLegalStateServiceProFactory workShiftBackToLegalStateServiceProFactory,
			IRequiredScheduleHelper requiredScheduleHelper,
			ScheduleOptimizerHelper scheduleOptimizerHelper,
			DoFullResourceOptimizationOneTime doFullResourceOptimizationOneTime)
		{
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_matrixListFactory = matrixListFactory;
			_periodExtractor = periodExtractor;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_scheduleMatrixOriginalStateContainerCreator = scheduleMatrixOriginalStateContainerCreator;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_workShiftBackToLegalStateServiceProFactory = workShiftBackToLegalStateServiceProFactory;
			_requiredScheduleHelper = requiredScheduleHelper;
			_scheduleOptimizerHelper = scheduleOptimizerHelper;
			_doFullResourceOptimizationOneTime = doFullResourceOptimizationOneTime;
		}

		public void Execute(IOptimizerOriginalPreferences optimizerOriginalPreferences, ISchedulingProgress backgroundWorker,
			ISchedulerStateHolder schedulerStateHolder, IList<IScheduleDay> selectedScheduleDays,
			IOptimizationPreferences optimizationPreferences, 
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var selectedPeriod = _periodExtractor.ExtractPeriod(selectedScheduleDays);
			if (!selectedPeriod.HasValue) return;

			bool lastCalculationState = schedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;

			if (lastCalculationState)
			{
				_resourceOptimizationHelperExtended().ResourceCalculateAllDays(backgroundWorker, false);
			}
#pragma warning disable 618
			_doFullResourceOptimizationOneTime.ExecuteIfNecessary();
#pragma warning restore 618


#pragma warning disable 618
			using (_resourceCalculationContextFactory.Create(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.Skills, false))
#pragma warning restore 618
			{
				var scheduleMatrixOriginalStateContainers =
					_scheduleMatrixOriginalStateContainerCreator.CreateScheduleMatrixOriginalStateContainers(schedulerStateHolder.Schedules, selectedScheduleDays, selectedPeriod.Value);
				IList<IDayOffTemplate> displayList = schedulerStateHolder.CommonStateHolder.ActiveDayOffs.ToList();
				_scheduleOptimizerHelper.DaysOffBackToLegalState(scheduleMatrixOriginalStateContainers,
					backgroundWorker, displayList[0], optimizerOriginalPreferences.SchedulingOptions,
					dayOffOptimizationPreferenceProvider, optimizationPreferences);

				_resourceOptimizationHelperExtended().ResourceCalculateMarkedDays(null,
					optimizerOriginalPreferences.SchedulingOptions.ConsiderShortBreaks, false);
				IList<IScheduleMatrixPro> matrixList = _matrixListFactory.CreateMatrixListForSelection(schedulerStateHolder.Schedules, selectedScheduleDays);

				backToLegalState(matrixList, schedulerStateHolder, optimizationPreferences, backgroundWorker,
					optimizerOriginalPreferences.SchedulingOptions, selectedPeriod.Value);
			}

			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}

		private void backToLegalState(IList<IScheduleMatrixPro> matrixList,
			ISchedulerStateHolder schedulerStateHolder,
			IOptimizationPreferences optimizationPreferences,
			ISchedulingProgress backgroundWorker,
			SchedulingOptions schedulingOptions,
			DateOnlyPeriod selectedPeriod)
		{
			if (matrixList == null) throw new ArgumentNullException(nameof(matrixList));
			if (schedulerStateHolder == null) throw new ArgumentNullException(nameof(schedulerStateHolder));
			if (backgroundWorker == null) throw new ArgumentNullException(nameof(backgroundWorker));
			foreach (var scheduleMatrix in matrixList)
			{
				var schedulePartModifyAndRollbackService =
					new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback,
						new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
				var workShiftBackToLegalStateServicePro = _workShiftBackToLegalStateServiceProFactory.Create();
				workShiftBackToLegalStateServicePro.Execute(scheduleMatrix, schedulingOptions, schedulePartModifyAndRollbackService);

				backgroundWorker.ReportProgress(1);
			}

			if (optimizationPreferences.General.UseShiftCategoryLimitations)
			{
				_requiredScheduleHelper.RemoveShiftCategoryBackToLegalState(matrixList, backgroundWorker, optimizationPreferences,
					schedulingOptions,
					selectedPeriod);
			}
		}
	}
}