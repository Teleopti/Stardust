using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
								IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
								IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator,
								SchedulingStateHolderAllSkillExtractor schedulingStateHolderAllSkillExtractor,
								IWorkShiftLegalStateDayIndexCalculator workShiftLegalStateDayIndexCalculator,
								IDeleteSchedulePartService deleteSchedulePartService,
								IScheduleService scheduleService,
								IEffectiveRestrictionCreator effectiveRestrictionCreator,
								IScheduleDayEquator scheduleDayEquator,
								IResourceOptimizationHelperExtended resouceOptimizationHelperExtended) :
			base(matrixListFactory,
								optimizerHelperHelper,
								schedulerStateHolder,
								daysOffBackToLegalState,
								optimizerHelper,
								resourceOptimizationHelper,
								scheduleDayChangeCallback,
								workShiftMinMaxCalculator,
								dailySkillForecastAndScheduledValueCalculator,
								schedulingStateHolderAllSkillExtractor,
								workShiftLegalStateDayIndexCalculator,
								deleteSchedulePartService,
								scheduleService,
								effectiveRestrictionCreator,
								scheduleDayEquator,
								resouceOptimizationHelperExtended)
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