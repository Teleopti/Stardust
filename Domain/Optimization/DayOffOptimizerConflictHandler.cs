using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class DayOffOptimizerConflictHandler : IDayOffOptimizerConflictHandler
    {
        private readonly IScheduleMatrixPro _scheduleMatrixPro;
        private readonly IScheduleService _scheduleService;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;

        public DayOffOptimizerConflictHandler(
            IScheduleMatrixPro scheduleMatrixPro, 
            IScheduleService scheduleService, 
            IEffectiveRestrictionCreator effectiveRestrictionCreator, 
            ISchedulePartModifyAndRollbackService rollbackService)
        {
            _scheduleMatrixPro = scheduleMatrixPro;
            _scheduleService = scheduleService;
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _rollbackService = rollbackService;
        }

        public bool HandleConflict(ISchedulingOptions schedulingOptions, DateOnly dateOnly)
        {
            var result = false;

            var scheduleDayBefore = _scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(-1));
            
            if(_scheduleMatrixPro.UnlockedDays.Contains(scheduleDayBefore))
            {
                _rollbackService.ClearModificationCollection();

                scheduleDayBefore.DaySchedulePart().DeleteMainShift(scheduleDayBefore.DaySchedulePart());

                _rollbackService.Modify(scheduleDayBefore.DaySchedulePart());

                var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDayBefore.DaySchedulePart(), schedulingOptions);
                result = _scheduleService.SchedulePersonOnDay(scheduleDayBefore.DaySchedulePart(), schedulingOptions, true, effectiveRestriction);

                if (result) return true;

                _rollbackService.Rollback();
            }

            var scheduleDayAfter = _scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(1));

            if(_scheduleMatrixPro.UnlockedDays.Contains(scheduleDayAfter))
            {
                _rollbackService.ClearModificationCollection();

                scheduleDayAfter.DaySchedulePart().DeleteMainShift(scheduleDayAfter.DaySchedulePart());

                _rollbackService.Modify(scheduleDayAfter.DaySchedulePart());

                var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDayAfter.DaySchedulePart(), schedulingOptions);
                result = _scheduleService.SchedulePersonOnDay(scheduleDayAfter.DaySchedulePart(), schedulingOptions, true, effectiveRestriction);

                if (!result)
                    _rollbackService.Rollback();
            }

            return result;
        }
    }
}
