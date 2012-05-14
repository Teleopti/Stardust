using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class SchedulePeriodShiftCategoryBackToLegalStateServiceBuilder : ISchedulePeriodShiftCategoryBackToLegalStateServiceBuilder
    {
        private readonly IScheduleMatrixValueCalculatorPro _scheduleMatrixValueCalculator;
        private readonly IScheduleDayService _scheduleDayService;

        public SchedulePeriodShiftCategoryBackToLegalStateServiceBuilder(
            IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator,
            IScheduleDayService scheduleDayService
            )
        {
            _scheduleMatrixValueCalculator = scheduleMatrixValueCalculator;
    	    _scheduleDayService = scheduleDayService;
        }

        public ISchedulePeriodShiftCategoryBackToLegalStateService Build(IScheduleMatrixPro scheduleMatrix, 
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            
            IRemoveShiftCategoryOnBestDateService removeShiftCategoryOnBestDateService =
                BuildRemoveShiftCategoryOnBestDateService(scheduleMatrix, _scheduleMatrixValueCalculator, _scheduleDayService);
            IRemoveShiftCategoryBackToLegalService removeShiftCategoryBackToLegalService =
                BuildRemoveShiftCategoryBackToLegalService(removeShiftCategoryOnBestDateService,
                                                           scheduleMatrix);
            ISchedulePeriodShiftCategoryBackToLegalStateService schedulePeriodBackToLegalStateService =
                BuildSchedulePeriodBackToLegalStateService(removeShiftCategoryBackToLegalService, _scheduleDayService);
            return schedulePeriodBackToLegalStateService;
        }

        public virtual IRemoveShiftCategoryOnBestDateService BuildRemoveShiftCategoryOnBestDateService(
            IScheduleMatrixPro scheduleMatrix,
            IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculatorPro,
            IScheduleDayService scheduleDayService)
        {
            return new RemoveShiftCategoryOnBestDateService(scheduleMatrix,
                                                            scheduleMatrixValueCalculatorPro,
                                                            scheduleDayService);
        }

        public virtual ISchedulePeriodShiftCategoryBackToLegalStateService BuildSchedulePeriodBackToLegalStateService(
            IRemoveShiftCategoryBackToLegalService removeShiftCategoryBackToLegalService,
            IScheduleDayService scheduleDayService)
        {
            return new SchedulePeriodShiftCategoryBackToLegalStateService(removeShiftCategoryBackToLegalService, scheduleDayService);
        }

        public virtual IRemoveShiftCategoryBackToLegalService BuildRemoveShiftCategoryBackToLegalService(
            IRemoveShiftCategoryOnBestDateService removeShiftCategoryBackToLegalService,
            IScheduleMatrixPro scheduleMatrix)
        {
            return new RemoveShiftCategoryBackToLegalService(removeShiftCategoryBackToLegalService, scheduleMatrix);
        }

        public virtual IScheduleDayService CreateScheduleService(
            IScheduleService scheduleService,
            IDeleteSchedulePartService deleteSchedulePartService,
            IResourceOptimizationHelper resourceOptimizationHelper, 
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            return new ScheduleDayService(
                scheduleService,
                deleteSchedulePartService,
                resourceOptimizationHelper, 
				effectiveRestrictionCreator,
				schedulePartModifyAndRollbackService);
        }
    }
}
