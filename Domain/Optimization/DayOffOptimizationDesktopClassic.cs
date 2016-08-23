using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationDesktopClassic : DayOffOptimizationDesktop
	{
		private readonly ClassicDaysOffOptimizationCommand _classicDaysOffOptimizationCommand;

		public DayOffOptimizationDesktopClassic(ClassicDaysOffOptimizationCommand classicDaysOffOptimizationCommand,
								IMatrixListFactory matrixListFactory,
								IOptimizerHelperHelper optimizerHelperHelper,
								Func<ISchedulerStateHolder> schedulerStateHolder,
								DaysOffBackToLegalState daysOffBackToLegalState,
								OptimizerHelperHelper optimizerHelper,
								IResourceOptimizationHelper resourceOptimizationHelper,
								Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
								IScheduleService scheduleService,
								IEffectiveRestrictionCreator effectiveRestrictionCreator,
								IScheduleDayEquator scheduleDayEquator,
								IResourceOptimizationHelperExtended resouceOptimizationHelperExtended,
								WorkShiftBackToLegalStateServiceProFactory workShiftBackToLegalStateServiceProFactory) :
			base(matrixListFactory,
								optimizerHelperHelper,
								schedulerStateHolder,
								daysOffBackToLegalState,
								optimizerHelper,
								resourceOptimizationHelper,
								scheduleDayChangeCallback,
								scheduleService,
								effectiveRestrictionCreator,
								scheduleDayEquator,
								resouceOptimizationHelperExtended,
								workShiftBackToLegalStateServiceProFactory)
		{
			_classicDaysOffOptimizationCommand = classicDaysOffOptimizationCommand;
		}

		protected override void Optimize(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization, DateOnlyPeriod selectedPeriod,
			ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_classicDaysOffOptimizationCommand.Execute(matrixOriginalStateContainerListForDayOffOptimization, selectedPeriod,
				optimizationPreferences, backgroundWorker, dayOffOptimizationPreferenceProvider);
		}
	}
}