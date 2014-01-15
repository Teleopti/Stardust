using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    /// <summary>
    /// Sets to 8-17 local on startdate
    /// </summary>
    public class SetupDateTimePeriodToDefaultLocalHours : ISetupDateTimePeriod
    {
        private DateTimePeriod _period;

        public SetupDateTimePeriodToDefaultLocalHours(DateTimePeriod defaultLocal, IScheduleDay scheduleDay, TimeZoneInfo info)
        {
            _period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(defaultLocal.LocalStartDateTime, defaultLocal.LocalEndDateTime, info);

            createFromScheduleDay(scheduleDay);
        }

        private void createFromScheduleDay(IScheduleDay scheduleDay)
        {
            if (ScheduleDayHasPersonAssignment(scheduleDay))
            {
                var timePeriod = scheduleDay.PersonAssignment().Period;
                _period = new DateTimePeriod(timePeriod.EndDateTime, timePeriod.EndDateTime.AddHours(1));
            }
        }

        private static bool ScheduleDayHasPersonAssignment(IScheduleDay scheduleDay)
        {
            return scheduleDay != null && scheduleDay.PersonAssignment() != null;
        }

        public DateTimePeriod Period
        {
            get { return _period; }
        }
    }
}