using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    public class SetupDateTimePeriodDefaultLocalHoursForActivities : ISetupDateTimePeriod
    {
        private readonly DateTimePeriod _period;
        private static DateTimePeriod _tp;

        public SetupDateTimePeriodDefaultLocalHoursForActivities(IScheduleDay scheduleDay)
        {
            HasDefaultValue = false;
            _period = GetPeriodFromScheduleDays(scheduleDay);
        }

        public SetupDateTimePeriodDefaultLocalHoursForActivities(IScheduleDay scheduleDay, DateTimePeriod defaultPeriod)
        {
            _tp = defaultPeriod;
            HasDefaultValue = true;
            _period = GetPeriodFromScheduleDays(scheduleDay);
        }

        private static DateTimePeriod GetPeriodFromScheduleDays(IScheduleDay scheduleDay)
        {
            var defaultPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(scheduleDay.Period.LocalStartDateTime.Add(TimeSpan.FromHours(8)), scheduleDay.Period.LocalStartDateTime.Add(TimeSpan.FromHours(17)));
            var dateTimePeriod = HasDefaultValue ? _tp : defaultPeriod;
            var setupDateTimePeriodToDefaultLocalHours =
                new SetupDateTimePeriodToDefaultLocalHours(
                    dateTimePeriod,
                    null,
                    scheduleDay.TimeZone);

            return setupDateTimePeriodToDefaultLocalHours.Period;
        }

        public DateTimePeriod Period
        {
            get { return _period; }
        }

        protected static bool HasDefaultValue
        {
            get;
            private set;
        }
    }
}
