using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class SchedulePeriodShiftCategoryBackToLegalStateServiceBuilder : ISchedulePeriodShiftCategoryBackToLegalStateServiceBuilder
    {
        private readonly IScheduleMatrixValueCalculatorPro _scheduleMatrixValueCalculator;
        private readonly Func<IScheduleDayService> _scheduleDayService;

        public SchedulePeriodShiftCategoryBackToLegalStateServiceBuilder(
            IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator,
            Func<IScheduleDayService> scheduleDayService
            )
        {
            _scheduleMatrixValueCalculator = scheduleMatrixValueCalculator;
    	    _scheduleDayService = scheduleDayService;
        }

        public ISchedulePeriodShiftCategoryBackToLegalStateService Build(IScheduleMatrixPro scheduleMatrix, 
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            
            IRemoveShiftCategoryOnBestDateService removeShiftCategoryOnBestDateService =
                BuildRemoveShiftCategoryOnBestDateService(scheduleMatrix, _scheduleMatrixValueCalculator, _scheduleDayService());
            IRemoveShiftCategoryBackToLegalService removeShiftCategoryBackToLegalService =
                BuildRemoveShiftCategoryBackToLegalService(removeShiftCategoryOnBestDateService,
                                                           scheduleMatrix);
            ISchedulePeriodShiftCategoryBackToLegalStateService schedulePeriodBackToLegalStateService =
                BuildSchedulePeriodBackToLegalStateService(removeShiftCategoryBackToLegalService, _scheduleDayService());
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
    }
}
