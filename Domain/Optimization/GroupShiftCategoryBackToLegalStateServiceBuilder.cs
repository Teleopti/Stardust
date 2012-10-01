using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupShiftCategoryBackToLegalStateServiceBuilder
    {
        IGroupShiftCategoryBackToLegalStateService Build(IScheduleMatrixPro scheduleMatrix,
                                                                        ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService);
    }

    public class GroupShiftCategoryBackToLegalStateServiceBuilder : IGroupShiftCategoryBackToLegalStateServiceBuilder
    {
        private readonly IScheduleMatrixValueCalculatorPro _scheduleMatrixValueCalculator;
        private readonly IGroupSchedulingService _scheduleService;
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
        private readonly IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;

        public GroupShiftCategoryBackToLegalStateServiceBuilder(IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator,
            IGroupSchedulingService scheduleService,
            IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
            IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup)
        {
            _scheduleMatrixValueCalculator = scheduleMatrixValueCalculator;
            _scheduleService = scheduleService;
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
            _groupOptimizerFindMatrixesForGroup = groupOptimizerFindMatrixesForGroup;
        }

        public IGroupShiftCategoryBackToLegalStateService Build(IScheduleMatrixPro scheduleMatrix,
                                                                ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            var removeShiftCategoryOnBestDateService =
                BuildRemoveGroupShiftCategoryOnBestDateService(scheduleMatrix, _scheduleMatrixValueCalculator, _scheduleService);
            var removeShiftCategoryBackToLegalService =
                BuildRemoveShiftCategoryBackToLegalService(removeShiftCategoryOnBestDateService,
                                                           scheduleMatrix);
            var schedulePeriodBackToLegalStateService =
                BuildSchedulePeriodBackToLegalStateService(removeShiftCategoryBackToLegalService, _scheduleService);
            return schedulePeriodBackToLegalStateService;
        }

        public virtual IRemoveShiftCategoryOnBestDateService BuildRemoveGroupShiftCategoryOnBestDateService(
            IScheduleMatrixPro scheduleMatrix,
            IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculatorPro,
            IGroupSchedulingService scheduleService)
        {
            return new RemoveGroupShiftCategoryOnBestDateService(scheduleMatrix,
                                                                 scheduleMatrixValueCalculatorPro,
                                                                 scheduleService, _groupOptimizerFindMatrixesForGroup);
        }

        public virtual IGroupShiftCategoryBackToLegalStateService BuildSchedulePeriodBackToLegalStateService(
            IRemoveShiftCategoryBackToLegalService removeShiftCategoryBackToLegalService,
            IGroupSchedulingService scheduleService)
        {
            return new GroupShiftCategoryBackToLegalStateService(removeShiftCategoryBackToLegalService, scheduleService, _groupPersonBuilderForOptimization);
        }

        public virtual IRemoveShiftCategoryBackToLegalService BuildRemoveShiftCategoryBackToLegalService(
            IRemoveShiftCategoryOnBestDateService removeShiftCategoryBackToLegalService,
            IScheduleMatrixPro scheduleMatrix)
        {
            return new RemoveShiftCategoryBackToLegalService(removeShiftCategoryBackToLegalService, scheduleMatrix);
        }
    }
}