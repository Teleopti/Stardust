using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
    public class SameShiftCategoryRestriction : IScheduleRestrictionStrategy
    {
		public IEffectiveRestriction ExtractRestriction(IList<DateOnly> dateOnlyList,
														IList<IScheduleMatrixPro> matrixList)
        {
            var restriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                       new EndTimeLimitation(),
                                                       new WorkTimeLimitation(), null, null, null,
                                                       new List<IActivityRestriction>());
            foreach (IScheduleMatrixPro matrix in matrixList)
            {
                foreach (DateOnly dateOnly in dateOnlyList)
                {
                    IScheduleDayPro schedule = matrix.GetScheduleDayByKey(dateOnly);
                    if (schedule == null)
                        continue;

                    IScheduleDay schedulePart = schedule.DaySchedulePart();
                    if (schedulePart.SignificantPart() == SchedulePartView.MainShift)
                    {
                        IPersonAssignment assignment = schedulePart.PersonAssignment();
                        if (assignment == null) continue;
                        IShiftCategory shiftCategory = assignment.ShiftCategory;
                        if (shiftCategory == null)
                            continue;
                        if (restriction.ShiftCategory == null)
                        {
                            restriction.ShiftCategory = shiftCategory;
                        }
                        else
                        {
                            if (restriction.ShiftCategory != shiftCategory)
								return new EffectiveRestriction(new StartTimeLimitation(),
													   new EndTimeLimitation(),
													   new WorkTimeLimitation(), null, null, null,
													   new List<IActivityRestriction>());
                        }
                    }
                }
            }
            return restriction;
        }
    }
}