using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class DayOffOptimizerConflictHandler : IDayOffOptimizerConflictHandler
    {
        private readonly IScheduleMatrixPro _scheduleMatrixPro;
        private readonly IScheduleService _scheduleService;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
    	private readonly IResourceCalculateDelayer _resourceCalculateDelayer;

    	public DayOffOptimizerConflictHandler(
            IScheduleMatrixPro scheduleMatrixPro, 
            IScheduleService scheduleService, 
            IEffectiveRestrictionCreator effectiveRestrictionCreator, 
            ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer)
        {
            _scheduleMatrixPro = scheduleMatrixPro;
            _scheduleService = scheduleService;
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _rollbackService = rollbackService;
        	_resourceCalculateDelayer = resourceCalculateDelayer;
        }

        public bool HandleConflict(SchedulingOptions schedulingOptions, DateOnly dateOnly)
        {
            var result = false;
            var scheduleDayBefore = _scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(-1));
            
            if(_scheduleMatrixPro.UnlockedDays.Contains(scheduleDayBefore))
            {
                _rollbackService.ClearModificationCollection();
            	IScheduleDay scheduleDay = scheduleDayBefore.DaySchedulePart();
				result = tryFix(scheduleDay, schedulingOptions);
                if (result) 
					return true;

                _rollbackService.Rollback();
                _resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, false);
            }

            var scheduleDayAfter = _scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(1));

            if(_scheduleMatrixPro.UnlockedDays.Contains(scheduleDayAfter))
            {
                _rollbackService.ClearModificationCollection();
				IScheduleDay scheduleDay = scheduleDayAfter.DaySchedulePart();
				result = tryFix(scheduleDay, schedulingOptions);
                if (!result)
                {
					_rollbackService.Rollback();
                    _resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null,false);
                }    
            }

            return result;
        }

		private bool tryFix(IScheduleDay scheduleDay, SchedulingOptions schedulingOptions)
		{
			scheduleDay.DeleteMainShift();
			_rollbackService.Modify(scheduleDay);
			var dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
			_resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, false);
			var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay, schedulingOptions);
			bool result = _scheduleService.SchedulePersonOnDay(scheduleDay.ReFetch(), schedulingOptions, effectiveRestriction, _resourceCalculateDelayer, _rollbackService);

			return result;
		}
    }
}
