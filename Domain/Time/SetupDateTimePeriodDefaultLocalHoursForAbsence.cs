using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Time
{
    public class SetupDateTimePeriodDefaultLocalHoursForAbsence: ISetupDateTimePeriod
    {
		private readonly IUserTimeZone _userTimeZone;

		public SetupDateTimePeriodDefaultLocalHoursForAbsence(IScheduleDay scheduleDay, IUserTimeZone userTimeZone)
        {
			_userTimeZone = userTimeZone;
			Period = getPeriodFromScheduleDays(scheduleDay);
        }

	    private DateTimePeriod getPeriodFromScheduleDays(IScheduleDay scheduleDay)
		{
			var timeZone = _userTimeZone.TimeZone();
		    var defaultPeriod =
			    TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
				    scheduleDay.Period.StartDateTimeLocal(timeZone).Add(TimeSpan.Zero),
				    scheduleDay.Period.StartDateTimeLocal(timeZone)
					    .Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59))), timeZone);
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