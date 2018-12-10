using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
    public class SameShiftRestriction : IScheduleRestrictionStrategy
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

					var schedulePart = schedule?.DaySchedulePart();
                    if (schedulePart?.SignificantPart() == SchedulePartView.MainShift)
                    {
                        var mainShift = schedulePart.GetEditorShift();
                        if (mainShift == null) continue;
                        if (restriction.CommonMainShift == null)
                        {
                            restriction.CommonMainShift = mainShift;
                        }
                        else
                        {
                            if (!_scheduleDayEquator.MainShiftBasicEquals(mainShift, restriction.CommonMainShift, schedulePart.TimeZone))
								return null;
                        }
                    }
                }
            }
            return restriction;
        }
    }
}
