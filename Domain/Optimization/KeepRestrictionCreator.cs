using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class KeepRestrictionCreator : IKeepRestrictionCreator
    {
        public IEffectiveRestriction CreateKeepShiftCategoryRestriction(IScheduleDay scheduleDay)
        {

            IShiftCategory shiftCategory = scheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory;
            return new EffectiveRestriction(
                new StartTimeLimitation(),
                new EndTimeLimitation(),
                new WorkTimeLimitation(), shiftCategory,
                null,
                null,
                new List<IActivityRestriction>());
        }


        public IEffectiveRestriction CreateKeepStartAndEndTimeRestriction(IScheduleDay scheduleDay)
        {
			IPerson person = scheduleDay.Person;
			TimePeriod timePeriod = scheduleDay.AssignmentHighZOrder().MainShift.ProjectionService().
                CreateProjection().Period().Value.TimePeriod(person.PermissionInformation.DefaultTimeZone());
            TimeSpan shiftStartTime = timePeriod.StartTime;
            TimeSpan shiftEndTime = timePeriod.EndTime;

            return new EffectiveRestriction(
                new StartTimeLimitation(shiftStartTime, shiftStartTime),
                new EndTimeLimitation(shiftEndTime, shiftEndTime),
                new WorkTimeLimitation(), null, null, null,
                new List<IActivityRestriction>());
        }
    }
}
