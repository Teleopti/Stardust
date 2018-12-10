using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
    public class SameEndTimeRestriction : IScheduleRestrictionStrategy
    {
        private readonly TimeZoneInfo _timeZone;

        public SameEndTimeRestriction(TimeZoneInfo timeZone)
        {
            _timeZone = timeZone;
        }

        public IEffectiveRestriction ExtractRestriction(IList<DateOnly> dateOnlyList, IList<IScheduleMatrixPro> matrixList)
        {
            var endTimeLimitation = new EndTimeLimitation();
			foreach (var matrix in matrixList)
            {
                foreach (var dateOnly in dateOnlyList)
                {
                    var schedule = matrix.GetScheduleDayByKey(dateOnly);
                    if (schedule == null)
                        continue;
					var scheduleDay = schedule.DaySchedulePart();
	                if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
	                {
		                var period = scheduleDay.ProjectionService().CreateProjection().Period();
		                if (period == null) continue;
		                if (endTimeLimitation.StartTime == null && endTimeLimitation.EndTime == null)
		                {
			                var timePeriod = period.Value.TimePeriod(_timeZone);
			                endTimeLimitation = new EndTimeLimitation(timePeriod.EndTime, timePeriod.EndTime);
		                }
		                else
		                {
			                var timePeriod = period.Value.TimePeriod(_timeZone);
			                if (endTimeLimitation.StartTime != timePeriod.EndTime ||
			                    endTimeLimitation.EndTime != timePeriod.EndTime)
				                return null;
		                }
	                }
                }
            }
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
													   endTimeLimitation,
													   new WorkTimeLimitation(), null, null, null,
													   new List<IActivityRestriction>());
            return restriction;
        }
    }
}
