using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
    public interface ISameShiftRestriction
    {
        IEffectiveRestriction ExtractRestriction(IList<DateOnly> dateOnlyList, IList<IScheduleMatrixPro> matrixList);
    }

    public class SameShiftRestriction : ISameShiftRestriction
    {
        private readonly IScheduleDayEquator _scheduleDayEquator;

        public SameShiftRestriction(IScheduleDayEquator scheduleDayEquator)
        {
            _scheduleDayEquator = scheduleDayEquator;
        }

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
                        var mainShift = schedulePart.GetEditorShift();
                        if (mainShift == null) continue;
                        if (restriction.CommonMainShift == null)
                        {
                            restriction.CommonMainShift = mainShift;
                        }
                        else
                        {
                            if (!_scheduleDayEquator.MainShiftBasicEquals(mainShift, restriction.CommonMainShift))
                                return null;
                        }
                    }
                }
            }
            return restriction;
        }
    }
}
