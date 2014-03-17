using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WeeklyRest
{
    public interface IDayOffWithLongestSpan
    {
        DateOnly GetDayOffWithLongestSpan(IList<IScheduleDay> scheduleDayList);
    }

    public class DayOffWithLongestSpan : IDayOffWithLongestSpan
    {
        public DateOnly GetDayOffWithLongestSpan(IList< IScheduleDay> scheduleDayList)
        {
            var timeSpan = TimeSpan.MinValue;
            if (scheduleDayList.Count > 0)
            {
                var result = scheduleDayList[0].PersonAssignment().Date;
                foreach (var scheduleDay in scheduleDayList)
                {
                    var scheduleDayPersonAssignment = scheduleDay.PersonAssignment();
                    var scheduleDayTimeSpan = scheduleDayPersonAssignment.DayOff().TargetLength;
                    if (timeSpan < scheduleDayTimeSpan)
                    {
                        timeSpan = scheduleDayTimeSpan;
                        result = scheduleDayPersonAssignment.Date;
                    }

                }
                return result;
            }
            return new DateOnly();
        }
    }
}