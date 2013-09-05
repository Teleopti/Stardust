using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

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

        public bool HandleConflict(ISchedulingOptions schedulingOptions, DateOnly dateOnly)
        {
            var result = false;
            var scheduleDayBefore = _scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(-1));
            
            if(_scheduleMatrixPro.UnlockedDays.Contains(scheduleDayBefore))
            {
                _rollbackService.ClearModificationCollection();
            	IScheduleDay scheduleDay = scheduleDayBefore.DaySchedulePart();
				result = tryFix(dateOnly, scheduleDay, schedulingOptions);
                if (result) 
					return true;

                _rollbackService.Rollback();
                _resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, _rollbackService.ModificationCollection.ToArray());
            }

            var scheduleDayAfter = _scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(1));

            if(_scheduleMatrixPro.UnlockedDays.Contains(scheduleDayAfter))
            {
                _rollbackService.ClearModificationCollection();
				IScheduleDay scheduleDay = scheduleDayAfter.DaySchedulePart();
				result = tryFix(dateOnly, scheduleDay, schedulingOptions);
                if (!result)
                {
					_rollbackService.Rollback();
                    _resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, _rollbackService.ModificationCollection.ToArray());
                }    
            }

            return result;
        }

		private bool tryFix(DateOnly dateOnly, IScheduleDay scheduleDay, ISchedulingOptions schedulingOptions)
		{
			var originalDay = (IScheduleDay)scheduleDay.Clone();
			scheduleDay.DeleteMainShift(scheduleDay);
			_rollbackService.Modify(scheduleDay);
			_resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null,
														new List<IScheduleDay>(), new List<IScheduleDay> { originalDay });
			var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay, schedulingOptions);
			bool result = _scheduleService.SchedulePersonOnDay(scheduleDay, schedulingOptions, effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService);

			return result;
		}
    }
}
