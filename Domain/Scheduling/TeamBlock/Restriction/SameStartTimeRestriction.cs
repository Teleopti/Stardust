using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
    public class SameStartTimeRestriction : IScheduleRestrictionStrategy
    {
        private readonly TimeZoneInfo _timeZone;

        public SameStartTimeRestriction(TimeZoneInfo timeZone)
        {
            _timeZone = timeZone;
        }

        public IEffectiveRestriction ExtractRestriction(IList<DateOnly> dateOnlyList,
                                                        IList<IScheduleMatrixPro> matrixList)
        {
            var startTimeLimitation = new StartTimeLimitation();
			
            foreach (IScheduleMatrixPro matrix in matrixList)
            {
                foreach (DateOnly dateOnly in dateOnlyList)
                {
                    IScheduleDayPro schedule = matrix.GetScheduleDayByKey(dateOnly);
                    if (schedule == null)
                        continue;
					var scheduleDay = schedule.DaySchedulePart();
	                if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
	                {
		                DateTimePeriod? period = scheduleDay.ProjectionService().CreateProjection().Period();
		                if (period == null) continue;
		                if (startTimeLimitation.StartTime == null && startTimeLimitation.EndTime == null)
		                {
			                TimePeriod timePeriod = period.Value.TimePeriod(_timeZone);
			                startTimeLimitation = new StartTimeLimitation(timePeriod.StartTime, timePeriod.StartTime);
		                }
		                else
		                {
			                TimePeriod timePeriod = period.Value.TimePeriod(_timeZone);
			                if (startTimeLimitation.StartTime != timePeriod.StartTime ||
			                    startTimeLimitation.EndTime != timePeriod.StartTime)
				                return null;
		                }
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