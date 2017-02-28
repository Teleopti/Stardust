using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    /// <summary>
    /// Sets to 8-17 local on startdate
    /// </summary>
    public class SetupDateTimePeriodToDefaultLocalHours : ISetupDateTimePeriod
    {
	    public SetupDateTimePeriodToDefaultLocalHours(DateTimePeriod defaultLocal, IScheduleDay scheduleDay, TimeZoneInfo info)
        {
            Period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(defaultLocal.StartDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone), defaultLocal.EndDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone), info);

            createFromScheduleDay(scheduleDay);
        }

        private void createFromScheduleDay(IScheduleDay scheduleDay)
        {
            if (scheduleDayHasPersonAssignment(scheduleDay))
            {
                var timePeriod = scheduleDay.PersonAssignment().Period;
                Period = new DateTimePeriod(timePeriod.EndDateTime, timePeriod.EndDateTime.AddHours(1));
            }
        }

        private static bool scheduleDayHasPersonAssignment(IScheduleDay scheduleDay)
        {
            return scheduleDay?.PersonAssignment() != null;
        }

        public DateTimePeriod Period { get; private set; }
    }
}