using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
  public class ShiftNudgeEarlier
    {
        private readonly Domain.Scheduling.Assignment.IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
      private readonly IScheduleService _scheduleService;
      private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;

      public ShiftNudgeEarlier(Domain.Scheduling.Assignment.IDeleteAndResourceCalculateService deleteAndResourceCalculateService, IScheduleService scheduleService, IEffectiveRestrictionCreator effectiveRestrictionCreator)
        {
            _deleteAndResourceCalculateService = deleteAndResourceCalculateService;
            _scheduleService = scheduleService;
          _effectiveRestrictionCreator = effectiveRestrictionCreator;
        }

      public bool Nudge(Interfaces.Domain.IScheduleDay scheduleDay, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions)
        {
            var shiftEnd =
                scheduleDay.PersonAssignment().ProjectionService().CreateProjection().Period().Value.EndDateTime;
            var scheduleDayList = new List<IScheduleDay> {scheduleDay};
            _deleteAndResourceCalculateService.DeleteWithResourceCalculation(scheduleDayList, rollbackService, true);
          var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay, schedulingOptions);
          effectiveRestriction.EndTimeLimitation.EndTime
            _scheduleService.SchedulePersonOnDay(scheduleDay,schedulingOptions)


            return false;
        }

    }
}
