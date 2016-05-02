﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
    public class NightlyRestRestrcition : IScheduleRestrictionStrategy
    {
		public IEffectiveRestriction ExtractRestriction(IList<DateOnly> dateOnlyList, IList<IScheduleMatrixPro> matrixList)
        {
            TimeSpan startTime = TimeSpan.FromDays(-10);
            TimeSpan endTime = TimeSpan.MaxValue;
            foreach (var day in dateOnlyList)
            {
                var matrixes = matrixList.Where(scheduleMatrixPro => scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(day)).ToList();
                var timePeriod = getTimePeriodForTeam(day, matrixes);
                if (timePeriod.StartTime > startTime)
                    startTime = timePeriod.StartTime;
                if (timePeriod.EndTime < endTime)
                    endTime = timePeriod.EndTime;
            }
            var endTimeLimit = new TimeSpan(1, 23, 59, 59);
            if (endTime > endTimeLimit)
                endTime = endTimeLimit;

            return new EffectiveRestriction(new StartTimeLimitation(startTime, null),
                                                       new EndTimeLimitation(null, endTime),
                                                       new WorkTimeLimitation(), null, null, null,
                                                       new List<IActivityRestriction>());
        }

        private readonly IAssignmentPeriodRule _nightlyRestRule;
	    private readonly ISchedulingOptions _schedulingOptions;

	    public NightlyRestRestrcition(IAssignmentPeriodRule nightlyRestRule, ISchedulingOptions schedulingOptions)
	    {
		    _nightlyRestRule = nightlyRestRule;
		    _schedulingOptions = schedulingOptions;
	    }


	    private TimePeriod getTimePeriodForTeam(DateOnly dateOnly, IEnumerable<IScheduleMatrixPro> matrixList)
        {
            TimeSpan startTime = TimeSpan.MinValue;
            TimeSpan endTime = TimeSpan.MaxValue;
            foreach (var matrix in matrixList)
            {
                var timePeriod = accessNightlyRestForEachMatrix(matrix, dateOnly);
                if (timePeriod.StartTime > startTime)
                    startTime = timePeriod.StartTime;
                if (timePeriod.EndTime < endTime)
                    endTime = timePeriod.EndTime;
            }

            return new TimePeriod(startTime, endTime);
        }

        private TimePeriod accessNightlyRestForEachMatrix(IScheduleMatrixPro matrix, DateOnly dateOnly)
        {
            var range = matrix.ActiveScheduleRange;
            var scheduleDay = range.ScheduledDay(dateOnly);
            if(_schedulingOptions.IsDayScheduled(scheduleDay) ) return new TimePeriod(TimeSpan.MinValue,TimeSpan.MaxValue );
            var dateTimePeriod = _nightlyRestRule.LongestDateTimePeriodForAssignment(range, dateOnly);

	        var timeZoneInfo = matrix.Person.PermissionInformation.DefaultTimeZone();
			DateTime lokalStartDateTime = dateTimePeriod.StartDateTimeLocal(timeZoneInfo);
			DateTime lokalEndDateTime = dateTimePeriod.EndDateTimeLocal(timeZoneInfo);

	        DateTime baseDateTime = dateOnly.Date;
			TimeSpan startTime = lokalStartDateTime.Subtract(baseDateTime);
	        TimeSpan endTime = lokalEndDateTime.Subtract(baseDateTime);
			return new TimePeriod(startTime, endTime);
        }
        
    }
}
