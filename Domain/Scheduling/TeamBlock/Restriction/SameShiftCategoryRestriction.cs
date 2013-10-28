using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
    public class SameShiftCategoryRestriction : IScheduleRestrictionStrategy
    {
        public IEffectiveRestriction ExtractRestriction(IList<DateOnly> dateOnlyList, IList<IScheduleMatrixPro> matrixList)
        {
            var restriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(),
                                                        new WorkTimeLimitation(), null, null, null,
                                                        new List<IActivityRestriction>());
            foreach (var matrix in matrixList)
            {
                foreach (var dateOnly in dateOnlyList)
                {
                    var schedule = matrix.GetScheduleDayByKey(dateOnly);
                    if (schedule == null)
                        continue;

                    var schedulePart = schedule.DaySchedulePart();
                    if (schedulePart.SignificantPart() == SchedulePartView.MainShift)
                    {
                        var assignment = schedulePart.PersonAssignment();
                        if (assignment == null) continue;
                        var shiftCategory = assignment.ShiftCategory;
                        if (shiftCategory == null)
                            continue;
                        if (restriction.ShiftCategory == null)
                        {
                            restriction.ShiftCategory = shiftCategory;
                        }
                        else
                        {
                            if (restriction.ShiftCategory != shiftCategory)
                                return null;
                        }
                    }
                }
            }
            return restriction;
        }
    }
}
