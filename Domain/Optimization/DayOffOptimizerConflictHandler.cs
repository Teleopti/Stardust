using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class DayOffOptimizerConflictHandler : IDayOffOptimizerConflictHandler
    {
        private readonly IScheduleMatrixPro _scheduleMatrixPro;
        private readonly IScheduleService _scheduleService;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly ISchedulingOptions _schedulingOptions;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;

        public DayOffOptimizerConflictHandler(IScheduleMatrixPro scheduleMatrixPro, IScheduleService scheduleService, IEffectiveRestrictionCreator effectiveRestrictionCreator, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
        {
            _scheduleMatrixPro = scheduleMatrixPro;
            _scheduleService = scheduleService;
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _schedulingOptions = schedulingOptions;
            _rollbackService = rollbackService;
        }

        public bool HandleConflict(DateOnly dateOnly)
        {
            var result = false;

            var scheduleDayBefore = _scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(-1));
            
            if(_scheduleMatrixPro.UnlockedDays.Contains(scheduleDayBefore))
            {
                _rollbackService.ClearModificationCollection();

                scheduleDayBefore.DaySchedulePart().DeleteMainShift(scheduleDayBefore.DaySchedulePart());

                _rollbackService.Modify(scheduleDayBefore.DaySchedulePart());

                var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDayBefore.DaySchedulePart(), _schedulingOptions);
                result = _scheduleService.SchedulePersonOnDay(scheduleDayBefore.DaySchedulePart(), true, effectiveRestriction);

                if (result) return true;

                _rollbackService.Rollback();
            }

            var scheduleDayAfter = _scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(1));

            if(_scheduleMatrixPro.UnlockedDays.Contains(scheduleDayAfter))
            {
                _rollbackService.ClearModificationCollection();

                scheduleDayAfter.DaySchedulePart().DeleteMainShift(scheduleDayAfter.DaySchedulePart());

                _rollbackService.Modify(scheduleDayAfter.DaySchedulePart());

                var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDayAfter.DaySchedulePart(), _schedulingOptions);
                result = _scheduleService.SchedulePersonOnDay(scheduleDayAfter.DaySchedulePart(), true, effectiveRestriction);

                if (!result)
                    _rollbackService.Rollback();
            }

            return result;
        }

        //public bool HandleConflict(DateOnly dateOnly)
        //{
        //    var result = false;

        //    if (!_scheduleMatrixPro.UserLockedDates.Contains(dateOnly.AddDays(-1)))
        //    {
        //        _rollbackService.ClearModificationCollection();

        //        var scheduleDayBefore = _scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(-1)).DaySchedulePart();
        //        scheduleDayBefore.DeleteMainShift(scheduleDayBefore);

        //        _rollbackService.Modify(scheduleDayBefore);
               
        //        var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDayBefore, _schedulingOptions);
        //        result = _scheduleService.SchedulePersonOnDay(scheduleDayBefore, true, effectiveRestriction);

        //        if (result) return true;

        //        _rollbackService.Rollback();
        //    }

        //    if (!_scheduleMatrixPro.UserLockedDates.Contains(dateOnly.AddDays(1)))
        //    {
        //        _rollbackService.ClearModificationCollection();

        //        var scheduleDayAfter = _scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(1)).DaySchedulePart();
        //        scheduleDayAfter.DeleteMainShift(scheduleDayAfter);

        //        _rollbackService.Modify(scheduleDayAfter);

        //        var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDayAfter, _schedulingOptions);
        //        result = _scheduleService.SchedulePersonOnDay(scheduleDayAfter, true, effectiveRestriction);

        //        if(!result)
        //            _rollbackService.Rollback();
        //    }
 
        //    return result;
        //}
    }
}
