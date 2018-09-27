using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    public class SetupDateTimePeriodDefaultLocalHoursForActivities : ISetupDateTimePeriod
    {
	    private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;

        public SetupDateTimePeriodDefaultLocalHoursForActivities(IScheduleDay scheduleDay, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
        {
	        _currentTeleoptiPrincipal = currentTeleoptiPrincipal;
	        Period = getPeriodFromScheduleDays(scheduleDay);
        }

        private DateTimePeriod getPeriodFromScheduleDays(IScheduleDay scheduleDay)
		{
			var defaultPeriod =
				TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
					scheduleDay.Period.StartDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone)
						.Add(TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultStartHour)),
					scheduleDay.Period.StartDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone)
						.Add(TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultEndHour)),
					_currentTeleoptiPrincipal.Current().Regional.TimeZone);
            var setupDateTimePeriodToDefaultLocalHours =
                new SetupDateTimePeriodToDefaultLocalHours(
					defaultPeriod,
                    scheduleDay.TimeZone);

            return setupDateTimePeriodToDefaultLocalHours.Period;
        }

        public DateTimePeriod Period { get; }
    }
}
