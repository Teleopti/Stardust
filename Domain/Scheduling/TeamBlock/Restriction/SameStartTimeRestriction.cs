using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
    public class SameStartTimeRestriction : IScheduleRestrictionStrategy 
    {
        private readonly TimeZoneInfo _timeZone;

        public SameStartTimeRestriction(TimeZoneInfo timeZone)
        {
            _timeZone = timeZone;
        }

        public IEffectiveRestriction ExtractRestriction(IList<DateOnly> dateOnlyList, IList<IScheduleMatrixPro> matrixList)
        {
            var startTimeLimitation = new StartTimeLimitation();
            foreach (var matrix in matrixList)
            {
                foreach (var dateOnly in dateOnlyList)
                {
                    var schedule = matrix.GetScheduleDayByKey(dateOnly);
                    if (schedule == null)
                        continue;

                    var period = schedule.DaySchedulePart().ProjectionService().CreateProjection().Period();
                    if (period == null) continue;
                    if (startTimeLimitation.StartTime == null && startTimeLimitation.EndTime == null)
                    {
                        var timePeriod = period.Value.TimePeriod(_timeZone);
                        startTimeLimitation = new StartTimeLimitation(timePeriod.StartTime, timePeriod.StartTime);
                    }
                    else
                    {
                        var timePeriod = period.Value.TimePeriod(_timeZone);
                        if (startTimeLimitation.StartTime != timePeriod.StartTime || startTimeLimitation.EndTime != timePeriod.StartTime)
                            return null;
                    }
                }
            }
            var restriction = new EffectiveRestriction(startTimeLimitation,
                                                       new EndTimeLimitation(),
                                                       new WorkTimeLimitation(), null, null, null,
                                                       new List<IActivityRestriction>());
            return restriction;
        }
    }
}
