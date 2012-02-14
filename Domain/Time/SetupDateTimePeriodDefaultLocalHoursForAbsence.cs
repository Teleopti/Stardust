using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    public class SetupDateTimePeriodDefaultLocalHoursForAbsence: ISetupDateTimePeriod
    {
        private readonly DateTimePeriod _period;

        public SetupDateTimePeriodDefaultLocalHoursForAbsence(IScheduleDay scheduleDay)
        {
            _period = GetPeriodFromScheduleDays(scheduleDay);
        }

        private static DateTimePeriod GetPeriodFromScheduleDays(IScheduleDay scheduleDay)
        {
            var defaultPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(scheduleDay.Period.LocalStartDateTime.Add(TimeSpan.Zero), scheduleDay.Period.LocalStartDateTime.Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59))));
            var setupDateTimePeriodToDefaultLocalHours =
                new SetupDateTimePeriodToDefaultLocalHours(
                    defaultPeriod,
                    null,
                    scheduleDay.TimeZone);

            return setupDateTimePeriodToDefaultLocalHours.Period;
        }

        public DateTimePeriod Period
        {
            get { return _period; }
        }
    }
}