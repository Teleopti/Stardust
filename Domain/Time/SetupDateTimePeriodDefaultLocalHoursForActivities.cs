using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Time
{
    public class SetupDateTimePeriodDefaultLocalHoursForActivities : ISetupDateTimePeriod
    {
		private readonly IUserTimeZone _userTimeZone;

        public SetupDateTimePeriodDefaultLocalHoursForActivities(IScheduleDay scheduleDay, IUserTimeZone userTimeZone)
        {
			_userTimeZone = userTimeZone;
	        Period = getPeriodFromScheduleDays(scheduleDay);
        }

        private DateTimePeriod getPeriodFromScheduleDays(IScheduleDay scheduleDay)
		{
			var timeZone = _userTimeZone.TimeZone();
			var defaultPeriod =
				TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
					scheduleDay.Period.StartDateTimeLocal(timeZone)
						.Add(TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultStartHour)),
					scheduleDay.Period.StartDateTimeLocal(timeZone)
						.Add(TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultEndHour)),
					timeZone);
            var setupDateTimePeriodToDefaultLocalHours =
                new SetupDateTimePeriodToDefaultLocalHours(
					defaultPeriod,
					_userTimeZone,
                    scheduleDay.TimeZone);

            return setupDateTimePeriodToDefaultLocalHours.Period;
        }

        public DateTimePeriod Period { get; }
    }
}
