using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle("Put the method override on IntradayOptimizer2Creator (and cleanup) when done and remove this", Toggles.ResourcePlanner_IntradayNoDailyValueCheck_42767)]
	public class IntradayOptimizeOnedayNoPeriodValueCheck : IntradayOptimizeOneday
	{
		public IntradayOptimizeOnedayNoPeriodValueCheck(IScheduleResultDailyValueCalculator dailyValueCalculator, 
			IScheduleService scheduleService,
			IOptimizationPreferences optimizerPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculation resourceOptimizationHelper,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IOptimizationLimits optimizationLimits,
			IScheduleMatrixOriginalStateContainer workShiftOriginalStateContainer,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService,
			IScheduleMatrixPro matrix,
			IIntradayOptimizeOneDayCallback intradayOptimizeOneDayCallback,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IUserTimeZone userTimeZone)
			: base(dailyValueCalculator, scheduleService, optimizerPreferences, rollbackService, resourceOptimizationHelper, effectiveRestrictionCreator, optimizationLimits, workShiftOriginalStateContainer, schedulingOptionsCreator, mainShiftOptimizeActivitySpecificationSetter, deleteAndResourceCalculateService, matrix, intradayOptimizeOneDayCallback, schedulingResultStateHolder, userTimeZone)
		{
		}

		protected override double? CalculatePeriodValue(DateOnly scheduleDay)
		{
			return 1;
		}

		protected override bool IsPeriodBetter(double newValidatedPeriodValue, double? oldPeriodValue)
		{
			return true;
		}
	}
}
